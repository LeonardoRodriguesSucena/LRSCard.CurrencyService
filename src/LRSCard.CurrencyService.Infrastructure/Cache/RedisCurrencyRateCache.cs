using LRSCard.CurrencyService.Application.Interfaces;
using LRSCard.CurrencyService.Domain;
using LRSCard.CurrencyService.Infrastructure.Options;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace LRSCard.CurrencyService.Infrastructure.Cache
{
    public class RedisCurrencyRateCache : ICurrencyRateCache
    {
        private readonly IDistributedCache _cache;
        private readonly CacheProviderOptions _cacheProviderOptions;

        public RedisCurrencyRateCache(IDistributedCache cache, IOptions<CacheProviderOptions> options)
        {
            _cache = cache;
            _cacheProviderOptions = options.Value;
        }

        public async Task<CurrencyRates?> GetAsync(DateTime date, string baseCurrency)
        {
            string key = GetKey(date, baseCurrency);
            try
            {
                var cached = await _cache.GetStringAsync(key);
                return cached != null
                    ? JsonSerializer.Deserialize<CurrencyRates>(cached)
                    : null;
            }
            catch (Exception err)
            {
                Console.WriteLine($"Error in getting cache for date:{date} and baseCurrency:{baseCurrency} : {err.Message}");
            }

            return null;
        }

        public async Task SetAsync(DateTime date, string baseCurrency, CurrencyRates rates)
        {
            string key = GetKey(date, baseCurrency);
            try
            {
                var json = JsonSerializer.Serialize(rates);
                await _cache.SetStringAsync(key, json, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(_cacheProviderOptions.CacheExpirationInDays)
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting cache for key '{key}': {ex.Message}");
            }
        }

        private static string GetKey(DateTime date, string baseCurrency) =>
            $"ExchangeRate:{baseCurrency}:{date:yyyy-MM-dd}";
    }
}
