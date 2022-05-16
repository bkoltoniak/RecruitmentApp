using RecruitmentApp.Core.Dto;
using RecruitmentApp.Core.Helpers;
using RecruitmentApp.Core.Interfaces;
using RecruitmentApp.Core.Models;
using RecruitmentApp.Data.Enums;
using System.Net.Http.Headers;
using System.Text.Json;

namespace RecruitmentApp.Data.Services;

public class EcbExchangeRateService : IExchangeRateApiService
{
    private readonly string _baseAddress = "https://sdw-wsrest.ecb.europa.eu/service/data/EXR/";
    private readonly string _exchangeRateType = "SP00";
    private readonly string _series = "A";
    private readonly string _basis = "D";
    private readonly string _dateFormat = "yyyy-MM-dd";
    private readonly string _accept = "application/json";
    private readonly HttpClient _httpClient;
    private readonly IRepository<ExchangeRate> _repository;

    public EcbExchangeRateService(IHttpClientFactory httpClientFactory, IRepository<ExchangeRate> repository)
    {
        _httpClient = httpClientFactory.CreateClient();
        _repository = repository;
    }

    public async Task<IEnumerable<ExchangeRateDto>> GetRatesAsync(string baseCurency, string quotedCurrency, DateTime startDate, DateTime endDate)
    {

        var baseCurencyParseResult = Enum.TryParse<EcbCurrency>(baseCurency, true, out var baseEcbCurrency);
        var quotedCurencyParseResult = Enum.TryParse<EcbCurrency>(quotedCurrency, true, out var quotedEcbCurrency);

        if(!baseCurencyParseResult && !quotedCurencyParseResult)
        {
            return Enumerable.Empty<ExchangeRateDto>();
        }

        var ratesFromDb = _repository
            .Get()
            .Where(e => e.BaseCurrency == baseCurency
                && e.QuotedCurrency == quotedCurrency
                && e.Date >= startDate
                && e.Date <= endDate)
            .OrderBy(e => e.Date)
            .ToList();

        var periodsWithoutRates = GetPeriodsWithoutRates(ratesFromDb, startDate, endDate);

        if (periodsWithoutRates.Any())
        {
            var ratesFromApi = new List<ExchangeRate>();

            foreach (var period in periodsWithoutRates)
            {
                var ratesForPeriod = await GetRatesAgainstEuro(baseCurency, quotedCurrency, period.Start, period.End);

                ratesForPeriod = await FillUnavailableRatesForPeriodAsync(ratesForPeriod, baseCurency, quotedCurrency, period.Start, period.End);

                ratesFromApi.AddRange(ratesForPeriod);
            }

            _repository.Add(ratesFromApi);
            await _repository.SaveAsync();

            var ratesFromDbAndApi = ratesFromDb.Union(ratesFromApi);

            return ratesFromDbAndApi
                .Select(e => new ExchangeRateDto
                {
                    BaseCurrency = e.BaseCurrency,
                    QuotedCurrency = e.QuotedCurrency,
                    Date = e.Date,
                    Rate = e.Rate
                })
                .OrderBy(e => e.Date);
        }

        return ratesFromDb
            .Select(e => new ExchangeRateDto
            {
                BaseCurrency = e.BaseCurrency,
                QuotedCurrency = e.QuotedCurrency,
                Date = e.Date,
                Rate = e.Rate
            })
            .OrderBy(e => e.Date);
    }

    // ECB only provides rates against euro.
    private async Task<IEnumerable<ExchangeRate>> GetRatesAgainstEuro(string baseCurrency, string quotedCurrency, DateTime startDate, DateTime endDate)
    {
        var rates = Enumerable.Empty<ExchangeRate>();

        if(baseCurrency == "EUR")
        {
            rates = await GetRatesFromApiAsync(baseCurrency, quotedCurrency, startDate, endDate);
        }
        else if(quotedCurrency == "EUR")
        {
            rates = (await GetRatesFromApiAsync(quotedCurrency, baseCurrency, startDate, endDate))
                .Select(r => new ExchangeRate(r.QuotedCurrency, r.BaseCurrency, Math.Round(1 / r.Rate, 4), r.Date));
        }
        else
        {
            var baseAgainstEuro = await GetRatesFromApiAsync("EUR", baseCurrency, startDate, endDate);

            var quotedAgainstEuro = await GetRatesFromApiAsync("EUR", quotedCurrency, startDate, endDate);

            rates = baseAgainstEuro.Select(r1 => new ExchangeRate(
                baseCurrency,
                quotedCurrency,
                Math.Round(quotedAgainstEuro.First(r2 => r2.Date == r1.Date).Rate / r1.Rate, 4),
                r1.Date));
        }

        return rates;
    }

    private IEnumerable<DatePeriod> GetPeriodsWithoutRates(IEnumerable<ExchangeRate> exchangeRates, DateTime startDate, DateTime endDate)
    {

        DateTime currentDay = startDate;
        var daysWithoutRate = new List<DateTime>();

        while (currentDay <= endDate)
        {

            if (!exchangeRates.Any(e => e.Date == currentDay))
            {
                daysWithoutRate.Add(currentDay);
            }

            currentDay = currentDay.AddDays(1);
        }


        DateTime? startPeriod = null;
        DateTime? endPeriod = null;

        for (var i = 0; i < daysWithoutRate.Count; i++)
        {
            if (startPeriod is null)
            {
                startPeriod = daysWithoutRate[i];
            }

            endPeriod = daysWithoutRate[i];

            if (i != daysWithoutRate.Count - 1 && daysWithoutRate[i].Date.AddDays(1) == daysWithoutRate[i + 1].Date)
            {
                continue;
            }

            yield return new DatePeriod
            {
                Start = startPeriod.Value,
                End = endPeriod.Value
            };

            startPeriod = null;
            endPeriod = null;
        }
    }

    private async Task<IEnumerable<ExchangeRate>> FillUnavailableRatesForPeriodAsync(
        IEnumerable<ExchangeRate> rates,
        string baseCurency,
        string quotedCurrency,
        DateTime startDate,
        DateTime endDate)
    {
        var result = new List<ExchangeRate>();
        ExchangeRate? rateForPreviousDate = null;
        ExchangeRate? rateForCurrentDay = null;
        DateTime currentDate = startDate;
        DateTime currentTime = currentDate + DateTime.UtcNow.TimeOfDay;
        
        while (currentDate <= endDate && currentDate <= DateTime.UtcNow.Date)
        {
            rateForCurrentDay = rates
                .Where(r => r.Date == currentDate)
                .FirstOrDefault();

            if (rateForCurrentDay is null)
            {
                if (currentDate == startDate)
                {
                    rateForCurrentDay = await GetLastAvailableRateForDateAsync(baseCurency, quotedCurrency, currentDate);
                    rateForCurrentDay.Date = currentDate;

                // Check if rate for today is already available.
                }else if(currentDate.Date == DateTime.UtcNow.Date)
                {
                    // ECB publishes their rates at ~4pm CET.
                    if (currentDate.Date + DateTime.UtcNow.TimeOfDay <= DateTime.UtcNow.Date.AddHours(16))
                    {
                        break;
                    }

                    rateForCurrentDay = ObjectHelper.DeepCopy(rateForPreviousDate);
                    rateForCurrentDay!.Date = currentDate;

                }
                else
                {
                    rateForCurrentDay = ObjectHelper.DeepCopy(rateForPreviousDate);
                    rateForCurrentDay!.Date = currentDate;
                }
            }

            result.Add(rateForCurrentDay!);
            rateForPreviousDate = rateForCurrentDay;
            rateForCurrentDay = null;
            currentDate = currentDate.AddDays(1);
            currentTime = currentDate + DateTime.UtcNow.TimeOfDay;
        }

        return result;
    }

    private async Task<ExchangeRate> GetLastAvailableRateForDateAsync(string baseCurrency, string quotedCurrency, DateTime date)
    {
        var rate = (await GetRatesAgainstEuro(baseCurrency, quotedCurrency, date, date)).FirstOrDefault();

        if (rate is null)
        {
            var previousDay = date.AddDays(-1);
            rate = await GetLastAvailableRateForDateAsync(baseCurrency, quotedCurrency, previousDay);
        }

        return rate;
    }

    private async Task<IEnumerable<ExchangeRate>> GetRatesFromApiAsync(string baseCurency, string quotedCurrency, DateTime startDate, DateTime endDate)
    {
        var request = GetMessageDescriptor(baseCurency, quotedCurrency, startDate, endDate);
        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            return Enumerable.Empty<ExchangeRate>();

        }

        var responseBody = await response.Content.ReadAsStringAsync();

        // Empty response when requesting for valid currency pair, but rate is unavailable e.g in holidays.
        if (string.IsNullOrEmpty(responseBody))
        {
            return Enumerable.Empty<ExchangeRate>();
        }

        return ParseResopnse(responseBody, baseCurency, quotedCurrency);
    }

    private HttpRequestMessage GetMessageDescriptor(string baseCurency, string quotedCurrency, DateTime startDate, DateTime endDate)
    {
        var uri = GetUri(baseCurency, quotedCurrency, startDate, endDate);
        var descriptor = new HttpRequestMessage(HttpMethod.Get, uri);
        descriptor.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(_accept, 1));

        return descriptor;
    }

    private string GetUri(string baseCurency, string quotedCurrency, DateTime startDate, DateTime endDate)
    {
        var key = string.Format("{0}.{1}.{2}.{3}.{4}",
            _basis,
            quotedCurrency.ToUpper(),
            baseCurency.ToUpper(),
            _exchangeRateType,
            _series);

        var queryParameters = string.Format("?detail=dataonly&startPeriod={0}&endPeriod={1}",
            startDate.ToString(_dateFormat),
            endDate.ToString(_dateFormat));

        return _baseAddress + key + queryParameters;
    }

    private IEnumerable<ExchangeRate> ParseResopnse(string response, string baseCurrency, string quotedCurrency)
    {
        using var json = JsonDocument.Parse(response);

        var observations = json
            .RootElement
            .GetProperty("dataSets")
            .EnumerateArray()
            .First()
            .GetProperty("series")
            .GetProperty("0:0:0:0:0")
            .GetProperty("observations")
            .EnumerateObject();

        var exchangeRates = observations.Select((observation, index) =>
        {
            decimal rate;

            try
            {
                rate = observation.Value.EnumerateArray().First().GetDecimal();
            }
            catch {
                rate = 0;
            }

            return new
            {
                index,
                rate
            };
        });

        var observationMetadata = json
            .RootElement
            .GetProperty("structure")
            .GetProperty("dimensions")
            .GetProperty("observation")
            .EnumerateArray()
            .First()
            .GetProperty("values")
            .EnumerateArray();

        var timePeriods = observationMetadata.Select((period, index) => new
        {
            index,
            date = period.GetProperty("start").GetDateTime()
        });

        var result = exchangeRates
            .Join(timePeriods, o => o.index, i => i.index, (o, i) => new ExchangeRate(
                baseCurrency,
                quotedCurrency,
                o.rate,
                i.date)
            )
            .ToList();

        return result;
    }

    private class DatePeriod
    {
        public DateTime Start { get; init; }
        public DateTime End { get; init; }
    }
}
