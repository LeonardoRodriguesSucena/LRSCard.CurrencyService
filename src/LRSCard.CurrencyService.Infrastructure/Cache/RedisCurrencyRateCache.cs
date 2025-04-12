using LRSCard.CurrencyService.Application.Interfaces;
using LRSCard.CurrencyService.Domain;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace LRSCard.CurrencyService.Infrastructure.Cache
{
    public class RedisCurrencyRateCache : ICurrencyRateCache
    {
        private readonly IDistributedCache _cache;

        public RedisCurrencyRateCache(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<CurrencyRates?> GetAsync(DateTime date, string baseCurrency)
        {
            string key = GetKey(date, baseCurrency);
            var cached = await _cache.GetStringAsync(key);
            return cached != null
                ? JsonSerializer.Deserialize<CurrencyRates>(cached)
                : null;
        }

        public async Task SetAsync(DateTime date, string baseCurrency, CurrencyRates rates)
        {
            string key = GetKey(date, baseCurrency);
            var json = JsonSerializer.Serialize(rates);

            await _cache.SetStringAsync(key, json, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24) // or configure
            });
        }

        private static string GetKey(DateTime date, string baseCurrency) =>
            $"ExchangeRate:{baseCurrency}:{date:yyyy-MM-dd}";
    }
}
