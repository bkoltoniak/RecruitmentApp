namespace RecruitmentApp.Core.Dto;

public class ExchangeRateDto
{
    public string? BaseCurrency { get; set; }

    public string? QuotedCurrency { get; set; }

    public decimal Rate { get; set; }

    public DateTime Date { get; set; }

}
