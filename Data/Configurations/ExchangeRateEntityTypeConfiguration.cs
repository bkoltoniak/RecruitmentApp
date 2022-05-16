using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecruitmentApp.Core.Models;

namespace RecruitmentApp.Data.Configurations;

public class ExchangeRateEntityTypeConfiguration : IEntityTypeConfiguration<ExchangeRate>
{
    public void Configure(EntityTypeBuilder<ExchangeRate> builder)
    {
        builder.HasIndex(e => new { e.BaseCurrency, e.QuotedCurrency });

        builder.Property(e => e.Rate).HasPrecision(10, 4);
    }
}
