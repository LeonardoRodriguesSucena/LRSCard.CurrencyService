using LRSCard.CurrencyService.Application.Common;
using LRSCard.CurrencyService.Application.Interfaces;
using LRSCard.CurrencyService.Application.Options;
using LRSCard.CurrencyService.Application.Requests;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRSCard.CurrencyService.Application
{
    public class CurrencyExchangeRateService : ICurrencyExchangeRateService
    {
        private readonly IExchangeRateProvider _exchangeRateProvider;
        private readonly HashSet<string> _currencyCodesNotAllowedInResponse;

        public CurrencyExchangeRateService(
            IExchangeRateProvider exchangeRateProvider, 
            IOptions<CurrencyRulesOptions> currencyRulesOptions) 
        {        
            _exchangeRateProvider = exchangeRateProvider;
            _currencyCodesNotAllowedInResponse = new HashSet<string>(currencyRulesOptions.Value.BlockedCurrencyCodes, StringComparer.OrdinalIgnoreCase);
        }

        public async Task<Domain.CurrencyRates> GetExchangeRate(GetExchangeRateRequest request)
        {
            var response = await this._exchangeRateProvider.GetExchangeRate(
                amount: request.Amount,
                date: request.Date,
                baseCurrency: request.BaseCurrency,
                symbols: request.Symbols
            );

            return response;
        }
        public async Task<Domain.CurrencyRates> GetCurrencyConvertion(GetCurrencyConversionRequest request)
        {
            var response = await this._exchangeRateProvider.GetExchangeRate(
                amount: request.Amount,
                baseCurrency: request.BaseCurrency,
                symbols: request.Symbols
            );

            response.Rates = response.Rates?
               .Where(r => !_currencyCodesNotAllowedInResponse.Contains(r.Key))
               .ToDictionary(r => r.Key, r => r.Value);


            return response;
        }

        public async Task<PaginationResult<Domain.CurrencyRates>> GetHistoricalExchangeRatePaginated(GetHistoricalExchangeRateRequest request)
        {
            //do the logic later
            return new PaginationResult<Domain.CurrencyRates>();
        }

    }



}
