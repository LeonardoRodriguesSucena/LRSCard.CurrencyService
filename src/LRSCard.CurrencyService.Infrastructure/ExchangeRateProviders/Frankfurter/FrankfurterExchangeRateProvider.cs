using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using LRSCard.CurrencyService.Application.Interfaces;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LRSCard.CurrencyService.Infrastructure.ExchangeRateProviders.Frankfurter
{
    public class FrankfurterExchangeRateProvider : IExchangeRateProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FrankfurterExchangeRateProvider> _logger;

        public FrankfurterExchangeRateProvider(HttpClient httpClient, ILogger<FrankfurterExchangeRateProvider> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(configuration["ExchangeProvider:Frankfurter:BaseUrl"]);
            
            //configuring the provider timeout
            string? timeoutSetting = configuration["ExchangeProviderResiliency:TimeoutInSeconds"];
            var timeoutSeconds = int.TryParse(timeoutSetting, out var parsedTimeout) ? parsedTimeout : 30;
            _httpClient.Timeout = TimeSpan.FromSeconds(timeoutSeconds);

            _logger = logger;
        }

        private async Task<FrankfurterAPIResponse> GetAPIExchangeRate(
            float? amount=null,
            DateTime? date = null,
            string? baseCurrency=null,            
            List<string>? symbols=null
         )
        {
            var queryParams = new Dictionary<string, string?>();
            string dateParam = date.HasValue ? date.Value.ToString("yyyy-MM-dd") : "latest";

            if (amount.HasValue)
                queryParams.Add("amount", amount.ToString());

            if (!String.IsNullOrEmpty(baseCurrency))
                queryParams.Add("base", baseCurrency.ToString());

            if (symbols != null && symbols.Count > 0)
                queryParams.Add("symbols", string.Join(",", symbols));

            string apiUrl = QueryHelpers.AddQueryString($"{this._httpClient.BaseAddress}{dateParam}", queryParams);

            try {                
                var response = await _httpClient.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();
                var contentStream = await response.Content.ReadAsStreamAsync();
                var getLastestResult = await JsonSerializer.DeserializeAsync<FrankfurterAPIResponse>(contentStream, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation( $"FrankfurterExchangeRateProvider GetAPIExchangeRate: RequestToProvider:{apiUrl} | Response: {JsonSerializer.Serialize(getLastestResult)}");

                return getLastestResult;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"FrankfurterExchangeRateProvider GetAPIExchangeRate:Error in getting exchange rate | Request: {apiUrl} | Error : {ex.Message}");
                throw;
            }            
        }

        public async Task<Domain.CurrencyRates> GetExchangeRate(
            float? amount = null,
            DateTime? date = null,
            string? baseCurrency = null,
            List<string>? symbols = null
         )
        {
            //calling the API
            FrankfurterAPIResponse response = await GetAPIExchangeRate(amount, date, baseCurrency, symbols);

            //mapping to Domain object
            if (response != null)
            {
                return new Domain.CurrencyRates
                {
                    Amount = response.Amount,
                    Base = response.Base,
                    Date = response.Date,
                    Rates = response.Rates
                };
            }
            else
            {
                throw new Exception("Error fetching exchange rates");
            }
        }
    }
    
}
