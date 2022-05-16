namespace RecruitmentApp.WebApi.ApiModels
{
    public class ExchangeRateResponseModel
    {
        public string? BaseCurrency { get; set; }
        public string? QuotedCurrency { get; set; }
        public IEnumerable<ExchangeRateValueResponseModel>? Values { get; set; }
    }
}
