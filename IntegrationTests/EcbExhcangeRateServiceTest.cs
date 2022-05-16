using Moq;
using RecruitmentApp.Core.Interfaces;
using RecruitmentApp.Core.Models;
using RecruitmentApp.Data.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace RecruitmentApp.IntegrationTests;

public class EcbExhcangeRateServiceTest : IDisposable
{

    private readonly Mock<IRepository<ExchangeRate>> _exchangeRateRepositoryMock;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly IExchangeRateApiService _sut;
    private readonly HttpClient _httpClient;


    public static IEnumerable<object[]> GetRatesForSingleDayTestData { get; } = new List<object[]>()
        {
            new object[] { "EUR", "USD", new DateTime(2022, 05, 10) },
            new object[] { "EUR", "USD", new DateTime(2021, 01, 15) },
            new object[] { "EUR", "USD", new DateTime(2020, 07, 20) },
            new object[] { "USD", "EUR", new DateTime(2022, 05, 10) },
            new object[] { "USD", "EUR", new DateTime(2021, 01, 15) },
            new object[] { "USD", "EUR", new DateTime(2020, 07, 20) },
            new object[] { "USD", "JPY", new DateTime(2022, 05, 10) },
            new object[] { "USD", "JPY", new DateTime(2022, 01, 15) },
            new object[] { "USD", "JPY", new DateTime(2020, 07, 20) },
        };

    public static IEnumerable<object[]> SinglePairForWeekendTestData { get; } = new List<object[]>()
        {
            new object[] { "EUR", "USD", new DateTime(2022, 05, 6), new DateTime(2022, 05, 8) },
            new object[] { "USD", "EUR", new DateTime(2022, 05, 6), new DateTime(2022, 05, 8) },
            new object[] { "USD", "JPY", new DateTime(2022, 05, 6), new DateTime(2022, 05, 8) },
        };

    public static IEnumerable<object[]> GerratesForPeriodTestData { get; } = new List<object[]>()
        {
            new object[] { "EUR", "USD", new DateTime(2020, 01, 01), new DateTime(2020, 01, 31) },
            new object[] { "USD", "EUR", new DateTime(2021, 02, 01), new DateTime(2021, 02, 28) },
            new object[] { "EUR", "JPY", new DateTime(2022, 03, 01), new DateTime(2022, 03, 31) },
        };

    private DateTime MonthPeriodStart { get; set; } = new DateTime(2022, 03, 01);

    private DateTime MonthPeriodEnd { get; set; } = new DateTime(2022, 03, 31);

    public EcbExhcangeRateServiceTest()
    {
        _httpClient = new HttpClient();

        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _httpClientFactoryMock
            .Setup(m => m.CreateClient(It.IsAny<string>()))
            .Returns(_httpClient);

        _exchangeRateRepositoryMock = new Mock<IRepository<ExchangeRate>>();
        _exchangeRateRepositoryMock
            .Setup(m => m.Get())
            .Returns(Enumerable.Empty<ExchangeRate>().AsQueryable());
        _exchangeRateRepositoryMock
            .Setup(r => r.SaveAsync())
            .Returns(Task.CompletedTask);

        _sut = new EcbExchangeRateService(_httpClientFactoryMock.Object, _exchangeRateRepositoryMock.Object);
    }


    ~EcbExhcangeRateServiceTest()
    {
        Dispose();
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        GC.SuppressFinalize(this);
    }

    [Theory]
    [MemberData(nameof(GetRatesForSingleDayTestData))]
    public void ExchangeRateApiService_GetRatesForSingleDay_SingleValidRateForDayIsPresent(string baseCurrency, string quotedCurrency, DateTime date)
    {
        var task = _sut.GetRatesAsync(baseCurrency, quotedCurrency, date, date);
        task.Wait();
        var rates = task.Result;

        Assert.NotEmpty(rates);
        Assert.Single(rates);

        var rate = rates.First();

        Assert.True(rate.BaseCurrency == baseCurrency);
        Assert.True(rate.QuotedCurrency == quotedCurrency);
        Assert.True(rate.Date == date);
        Assert.True(rate.Rate != 0);
    }

    [Theory]
    [MemberData(nameof(SinglePairForWeekendTestData))]
    public void ExchangeRateApiService_GetRatesForWeekend_SaturdayAndSundayRatesAreEqualFridayRate(string baseCurrency, string quotedCurrency, DateTime weekendStart, DateTime weekendEnd)
    {
        var task = _sut.GetRatesAsync(baseCurrency, quotedCurrency, weekendStart, weekendEnd);
        task.Wait();
        var rates = task.Result;

        Assert.True(rates.Count() == 3);
        var firdayRate = rates.First().Rate;
        Assert.True(rates.Where(r => r.Rate == firdayRate).Count() == 3);

    }

    [Theory]
    [MemberData(nameof(GerratesForPeriodTestData))]
    public void ExchangeRateApiService_GetRatesForPeriod_RatesNumberIsEqualNumberOfDaysInPeirod(string baseCurrency, string quotedCurrency, DateTime periodStart, DateTime periodEnd)
    {
        var task = _sut.GetRatesAsync(baseCurrency, quotedCurrency, periodStart, periodEnd);
        task.Wait();
        var rates = task.Result;
        var totalDays = (periodEnd - periodStart).TotalDays + 1;

        Assert.True(rates.Count() == totalDays);
    }
}
