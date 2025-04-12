using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using LRSCard.CurrencyService.Application.Interfaces;
using Microsoft.AspNetCore.WebUtilities;

namespace LRSCard.CurrencyService.Infrastructure.ExchangeRateProviders.Frankfurter
{
    public class FrankfurterExchangeRateProvider : IExchangeRateProvider
    {
        private readonly HttpClient _httpClient;
        
        public FrankfurterExchangeRateProvider(HttpClient httpClient)
        {
            _httpClient = httpClient;            
        }

        private async Task<FrankfurterAPIResponse> GetAPIExchangeRate(
            float? amount=null,
            DateTime? date = null,
            string? baseCurrency=null,            
            List<string>? symbols=null
         )
        {
            string dateParam = date.HasValue ? date.Value.ToString("yyyy-MM-dd"):"latest";
            var queryParams = new Dictionary<string, string?>();
            if(amount.HasValue) 
                queryParams.Add("amount", amount.ToString());
            
            if (!String.IsNullOrEmpty(baseCurrency)) 
                queryParams.Add("base", baseCurrency.ToString());

            if(symbols != null && symbols.Count > 0)
                queryParams.Add("symbols", string.Join(",", symbols));
            
            string apiUrl = QueryHelpers.AddQueryString($"{this._httpClient.BaseAddress}{dateParam}", queryParams);

            var response = await _httpClient.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();
            var contentStream = await response.Content.ReadAsStreamAsync();
            var getLastestResult = await JsonSerializer.DeserializeAsync<FrankfurterAPIResponse>(contentStream, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return getLastestResult;
        }

        public Task<Domain.CurrencyRates> GetExchangeRate(
            float? amount = null,
            DateTime? date = null,
            string? baseCurrency = null,
            List<string>? symbols = null
         )
        {
            //calling the API
            //add polly here afterwards
            FrankfurterAPIResponse response = GetAPIExchangeRate(amount, date, baseCurrency, symbols).Result;

            //mapping to Domain object
            if (response != null)
            {
                return Task.FromResult(new Domain.CurrencyRates
                {
                    Amount = response.Amount,
                    Base = response.Base,
                    Date = response.Date,
                    Rates = response.Rates
                });
            }
            else
            {
                throw new Exception("Error fetching exchange rates");
            }
        }
    }
    
}
