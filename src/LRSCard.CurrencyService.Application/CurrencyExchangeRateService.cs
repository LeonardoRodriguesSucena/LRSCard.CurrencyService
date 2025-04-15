using LRSCard.CurrencyService.Application.Common;
using LRSCard.CurrencyService.Application.Interfaces;
using LRSCard.CurrencyService.Application.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using LRSCard.CurrencyService.Application.Requests;
using LRSCard.CurrencyService.Domain;
using System.Net.Http.Json;
using System.Text.Json;

namespace LRSCard.CurrencyService.Application
{
    public class CurrencyExchangeRateService : ICurrencyExchangeRateService
    {
        private readonly IExchangeRateProvider _exchangeRateProvider;
        private readonly ICurrencyRateCache _currencyRateCache;
        private readonly HashSet<string> _currencyCodesNotAllowedInResponse;
        private readonly ILogger<CurrencyExchangeRateService> _logger;

        public CurrencyExchangeRateService(
            IExchangeRateProvider exchangeRateProvider,
            ICurrencyRateCache currencyRateCache,
            IOptions<CurrencyRulesOptions> currencyRulesOptions,
            ILogger<CurrencyExchangeRateService> logger) 
        {        
            _exchangeRateProvider = exchangeRateProvider;
            _currencyRateCache = currencyRateCache;
            _currencyCodesNotAllowedInResponse = new HashSet<string>(currencyRulesOptions.Value.BlockedCurrencyCodes, StringComparer.OrdinalIgnoreCase);
            _logger = logger;
        }

        /// <summary>
        /// Retrieves exchange rates for a specific date or the latest available rates.
        /// </summary>
        /// <param name="request">
        /// The request object containing the parameters:
        /// - <c>Amount</c>: (optional) The amount to convert. Default is 1.
        /// - <c>Date</c>: (optional) The specific date for historical data. If null, the latest rates will be fetched.
        /// - <c>BaseCurrency</c>: (required) The base currency code (e.g., "USD", "EUR").
        /// - <c>Symbols</c>: (optional) A list of target currency codes to filter the results.
        /// </param>
        /// <returns>
        /// A <see cref="Domain.CurrencyRates"/> object containing the exchange rate information for the given parameters.
        /// </returns>
        public async Task<Domain.CurrencyRates> GetExchangeRate(GetExchangeRateRequest request)
        {
            Domain.CurrencyRates? response = null;
            try { 
                response = await this._exchangeRateProvider.GetExchangeRate(
                    amount: request.Amount,
                    date: request.Date,
                    baseCurrency: request.BaseCurrency,
                    symbols: request.Symbols
                );

                // Make sure the result keeps the original requested date.
                // The provider may return data for the last business day if the date falls on a weekend or holiday.
                // This helps avoid confusion when showing the result.
                response.Date = request.Date.HasValue? request.Date : DateTime.Now;

                //Log request and response
                _logger.LogInformation("GetExchangeRate | Request: {Request} | Response: {Response}", 
                    JsonSerializer.Serialize(request), 
                    JsonSerializer.Serialize(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetExchangeRate | Request: {Request} | Response: {Response}", 
                    JsonSerializer.Serialize(request), 
                    (response != null)? JsonSerializer.Serialize(response): "null");

                throw;
            }

            return response;
        }

        /// <summary>
        /// Retrieves current convertion for BaseCurrency to Symbols informed.
        /// </summary>
        /// <param name="request">
        /// The request object containing the parameters:
        /// - <c>Amount</c>: (required) The amount to convert.
        /// - <c>BaseCurrency</c>: (required) The base currency code (e.g., "USD", "EUR").
        /// - <c>Symbols</c>: (required) A list of target currency codes to filter the results.
        /// </param>
        /// <returns>
        /// A <see cref="Domain.CurrencyRates"/> object containing the exchange rate information for the given parameters.
        /// </returns>
        public async Task<Domain.CurrencyRates> GetCurrencyConvertion(GetCurrencyConversionRequest request)
        {
            Domain.CurrencyRates response = null;

            try
            {
                response = await this._exchangeRateProvider.GetExchangeRate(
                amount: request.Amount,
                baseCurrency: request.BaseCurrency,
                symbols: request.Symbols
                );

                //removing not alowed currency codes
                response.Rates = response.Rates?
                   .Where(r => !_currencyCodesNotAllowedInResponse.Contains(r.Key))
                   .ToDictionary(r => r.Key, r => r.Value);

                // Make sure the result keeps the original requested date, in this case, the lastest.
                // The provider may return data for the last business day if the date falls on a weekend or holiday.
                // This helps avoid confusion when showing the result.
                response.Date = DateTime.Now;

                //Log request and response
                _logger.LogInformation("GetCurrencyConvertion | Request: {Request} | Response: {Response}",
                    JsonSerializer.Serialize(request),
                    JsonSerializer.Serialize(response));

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetCurrencyConvertion | Request: {Request} | Response: {Response}",
                  JsonSerializer.Serialize(request),
                  (response != null)?JsonSerializer.Serialize(response): null);

                throw;
            }            
        }

        /// <summary>
        /// Retrieves exchange rates for a specific date range.
        /// </summary>
        /// <param name="request">
        /// The request object containing the parameters:
        /// - <c>Amount</c>: (optional) The amount to convert. Default is 1.
        /// - <c>InitialDate</c>: (required) Initial Date.
        /// - <c>EndDate</c>: (required) End Date.
        /// - <c>BaseCurrency</c>: (required) The base currency code (e.g., "USD", "EUR").
        /// - <c>Symbols</c>: A list of target currency codes to filter the results.
        /// - <c>Pagination</c>: Pagination configuration.
        /// </param>
        /// <returns>
        /// Paginated CurrencyRates PaginationResult<CurrencyRates>.
        /// </returns>
        public async Task<PaginationResult<CurrencyRates>> GetHistoricalExchangeRatePaginated(GetHistoricalExchangeRateRequest request)
        {
            PaginationResult<CurrencyRates> response = null;

            try
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

                    //Ensuring that date requested is the date that will be shown in the result
                    //On weekends or holidays, the quotation date is the last business day
                    //So it can cause confusion
                    result.Date = date;

                    await _currencyRateCache.SetAsync(date, request.BaseCurrency, result);
                    results.Add(result);
                }

                response = new PaginationResult<CurrencyRates>
                {
                    Page = request.Pagination.Page,
                    PageSize = request.Pagination.PageSize,
                    TotalCount = allDates.Count,
                    Items = results
                };

                //Log request and response
                _logger.LogInformation("GetHistoricalExchangeRatePaginated | Request: {Request} | Response: {Response}",
                    JsonSerializer.Serialize(request),
                    JsonSerializer.Serialize(response));
                
                return response;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "GetHistoricalExchangeRatePaginated | Request: {Request} | Response: {Response}",
                    JsonSerializer.Serialize(request),
                    (response != null) ? JsonSerializer.Serialize(response) : null);

                throw;
            }

            
        }
    }



}
