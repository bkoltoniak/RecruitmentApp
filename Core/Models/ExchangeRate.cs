namespace RecruitmentApp.Core.Models;

public class ExchangeRate : AuditableEntity
{
    public string BaseCurrency { get; set; }

    public string QuotedCurrency { get; set; }

    public decimal Rate { get; set; }

    public DateTime Date { get; set; }

    public ExchangeRate(string baseCurrency, string quotedCurrency, decimal rate, DateTime date)
    {
        BaseCurrency = baseCurrency;
        QuotedCurrency = quotedCurrency;
        Rate = rate;
        Date = date;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private ExchangeRate()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
    }

}
