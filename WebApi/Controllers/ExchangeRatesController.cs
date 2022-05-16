using Microsoft.AspNetCore.Mvc;
using RecruitmentApp.Core.Dto;
using RecruitmentApp.Core.Interfaces;
using RecruitmentApp.WebApi.ApiModels;
using RecruitmentApp.WebApi.Attributes;

namespace RecruitmentApp.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[RequireApiKey]
public class ExchangeRatesController : ControllerBase
{

    private readonly IExchangeRateApiService _exchangeRateApiService;
    private readonly IChacheService _chacheService;

    public ExchangeRatesController(IExchangeRateApiService exchangeRateApiService, IChacheService chacheService)
    {
        _exchangeRateApiService = exchangeRateApiService;
        _chacheService = chacheService;
    }

    [HttpPost("GetRates")]
    public async Task<ActionResult<IEnumerable<ExchangeRateResponseModel>>> GetRates(GetRatesRequestModel model)
    {
        if(model.startDate > DateTime.UtcNow.Date)
        {
            return NotFound();
        }

        if(model.startDate.Date > model.endDate.Date)
        {
            ModelState.AddModelError(nameof(model.startDate), "Start date cannot be greather than end date.");
            return ValidationProblem(ModelState);
        }

        var responseData = _chacheService.GetValueOrDefault<List<ExchangeRateResponseModel>>(model);

        if (responseData is null)
        {
            responseData = new List<ExchangeRateResponseModel>();

            foreach (var currencyPair in model.currencyCodes!)
            {
                var exchangeRateDtos = await _exchangeRateApiService
                    .GetRatesAsync(
                    currencyPair.Key.ToUpper(),
                    currencyPair.Value.ToUpper(),
                    model.startDate.Date,
                    model.endDate.Date);

                var exchangeRates = new ExchangeRateResponseModel()
                {
                    BaseCurrency = currencyPair.Value.ToUpper(),
                    QuotedCurrency = currencyPair.Key.ToUpper(),
                    Values = exchangeRateDtos.Select(r => new ExchangeRateValueResponseModel { Date = r.Date, Rate = r.Rate })
                };

                responseData.Add(exchangeRates);
            }

            _chacheService.Set(model, responseData);
        }

        return responseData;
    }
}
