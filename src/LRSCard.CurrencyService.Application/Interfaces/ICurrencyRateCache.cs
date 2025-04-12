using LRSCard.CurrencyService.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRSCard.CurrencyService.Application.Interfaces
{
    public interface ICurrencyRateCache
    {
        Task<CurrencyRates?> GetAsync(DateTime date, string baseCurrency);
        Task SetAsync(DateTime date, string baseCurrency, CurrencyRates rates);
    }


}
