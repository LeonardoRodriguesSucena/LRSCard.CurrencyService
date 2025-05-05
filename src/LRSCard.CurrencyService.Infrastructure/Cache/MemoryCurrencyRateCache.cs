using LRSCard.CurrencyService.Application.Interfaces;
using LRSCard.CurrencyService.Domain;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;

namespace LRSCard.CurrencyService.Infrastructure.Cache
{
    public class MemoryCurrencyRateCache : ICurrencyRateCache
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<MemoryCurrencyRateCache> _logger;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromDays(1); 

        public MemoryCurrencyRateCache(IMemoryCache cache, ILogger<MemoryCurrencyRateCache> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public Task<CurrencyRates?> GetAsync(DateTime date, string baseCurrency)
        {
            string key = GetKey(date, baseCurrency);
            try
            {
                if (_cache.TryGetValue(key, out CurrencyRates rates))
                {
                    return Task.FromResult<CurrencyRates?>(rates);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"MemoryCurrencyRateCache GetAsync: Error retrieving cache for key '{key}': {ex.Message}");
            }

            return Task.FromResult<CurrencyRates?>(null);
        }

        public Task SetAsync(DateTime date, string baseCurrency, CurrencyRates rates)
        {
            string key = GetKey(date, baseCurrency);
            try
            {
                _cache.Set(key, rates, _cacheExpiration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"MemoryCurrencyRateCache SetAsync: Error setting cache for key '{key}': {ex.Message}");
            }

            return Task.CompletedTask;
        }

        private static string GetKey(DateTime date, string baseCurrency) =>
            $"ExchangeRate:{baseCurrency}:{date:yyyy-MM-dd}";
    }
}