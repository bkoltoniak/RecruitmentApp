using RecruitmentApp.Core.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecruitmentApp.Core.Interfaces;

public interface IExchangeRateApiService
{
    Task<IEnumerable<ExchangeRateDto>> GetRatesAsync(string baseCurency, string quotedCurrency, DateTime startDate, DateTime endDate);
}
