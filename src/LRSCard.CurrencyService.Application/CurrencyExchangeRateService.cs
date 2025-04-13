using LRSCard.CurrencyService.Application.Common;
using LRSCard.CurrencyService.Application.Interfaces;
using LRSCard.CurrencyService.Application.Options;
using LRSCard.CurrencyService.Application.Requests;
using LRSCard.CurrencyService.Domain;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LRSCard.CurrencyService.Application
{
    public class CurrencyExchangeRateService : ICurrencyExchangeRateService
    {
        private readonly IExchangeRateProvider _exchangeRateProvider;
        private readonly ICurrencyRateCache _currencyRateCache;
        private readonly HashSet<string> _currencyCodesNotAllowedInResponse;

        public CurrencyExchangeRateService(
            IExchangeRateProvider exchangeRateProvider,
            ICurrencyRateCache currencyRateCache,
            IOptions<CurrencyRulesOptions> currencyRulesOptions) 
        {        
            _exchangeRateProvider = exchangeRateProvider;
            _currencyRateCache = currencyRateCache;
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

        public async Task<PaginationResult<CurrencyRates>> GetHistoricalExchangeRatePaginated(GetHistoricalExchangeRateRequest request)
        {
            //creating a list with all days
            int dateRangeInDays = (request.EndDate - request.InitialDate).Days + 1;

            var allDates = new List<DateTime>();
            for (int i = 0; i < dateRangeInDays; i++)
                allDates.Add(request.InitialDate.AddDays(i));

            //filtering just the dates that will be returned, so just these we need to get the rates
            var pagedDates = allDates
                .Skip((request.Pagination.Page - 1) * request.Pagination.PageSize)
                .Take(request.Pagination.PageSize);

            var results = new List<CurrencyRates>();
            foreach (var date in pagedDates)
            {
                var cached = await _currencyRateCache.GetAsync(date, request.BaseCurrency);
                if (cached != null)
                {
                    results.Add(cached);
                    continue;
                }

                var result = await _exchangeRateProvider.GetExchangeRate(
                    date: date,
                    baseCurrency: request.BaseCurrency
                );

                await _currencyRateCache.SetAsync(date, request.BaseCurrency, result);
                results.Add(result);
            }

            return new PaginationResult<CurrencyRates>
            {
                Page = request.Pagination.Page,
                PageSize = request.Pagination.PageSize,
                TotalCount = allDates.Count,
                Items = results
            };
        }
    }



}
