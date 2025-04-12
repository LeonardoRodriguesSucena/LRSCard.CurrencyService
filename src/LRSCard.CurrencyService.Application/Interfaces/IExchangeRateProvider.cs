using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRSCard.CurrencyService.Application.Interfaces
{
    public interface IExchangeRateProvider
    {
        Task<LRSCard.CurrencyService.Domain.CurrencyRates> GetExchangeRate(
            float? amount = null,
            DateTime? date = null,
            string? baseCurrency = null,
            List<string>? symbols = null
            );
    }
}
