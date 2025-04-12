using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LRSCard.CurrencyService.Application.Common;
using LRSCard.CurrencyService.Application.Requests;

namespace LRSCard.CurrencyService.Application.Interfaces
{
    public interface ICurrencyExchangeRateService
    {
        Task<Domain.CurrencyRates> GetExchangeRate(GetExchangeRateRequest request);
        Task<Domain.CurrencyRates> GetCurrencyConvertion(GetCurrencyConversionRequest request);
        Task<PaginationResult<Domain.CurrencyRates>> GetHistoricalExchangeRatePaginated(GetHistoricalExchangeRateRequest request);

    }
}
