using System.Net.Http;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using LRSCard.CurrencyService.Application.Interfaces;
using LRSCard.CurrencyService.Infrastructure.ExchangeRateProviders.Frankfurter;
using LRSCard.CurrencyService.Infrastructure.Options;
using Microsoft.Extensions.Options;
using LRSCard.CurrencyService.Infrastructure.Cache;
using LRSCard.CurrencyService.Application.Common;
using LRSCard.CurrencyService.Infrastructure.Factories;
using Microsoft.Extensions.Logging;

namespace LRSCard.CurrencyService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var resiliencyConfig = configuration
                .GetSection("ExchangeProviderResiliency")
                .Get<ExchangeProviderResiliencyOptions>() ?? new ExchangeProviderResiliencyOptions();

            services.AddHttpClient<FrankfurterExchangeRateProvider>()
            .AddPolicyHandler((sp, _) =>
            {
                var logger = sp.GetRequiredService<ILogger<FrankfurterExchangeRateProvider>>();
                return GetRetryPolicy(resiliencyConfig, logger);
            })
            .AddPolicyHandler((sp, _) =>
            {
                var logger = sp.GetRequiredService<ILogger<FrankfurterExchangeRateProvider>>();
                return GetCircuitBreakerPolicy(resiliencyConfig, logger);
            });

            // Register all currency providers, we can add more later
            services.AddScoped<FrankfurterExchangeRateProvider>();

            // Registering the CurrencyProviderFactory
            services.AddScoped<ICurrencyProviderFactory, CurrencyProviderFactory>();

            //Redis
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration["Redis:ConnectionString"] ?? "localhost:6379";
            });
            services.Configure<CacheProviderOptions>(configuration.GetSection("CacheProviderOptions"));
            services.AddSingleton<ICurrencyRateCache, RedisCurrencyRateCache>();

            return services;
        }

        /// This method configures a retry policy using Polly for transient HTTP errors.
        /// It retries the request up to 3 times with an exponential backoff strategy.(Default values)
        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(ExchangeProviderResiliencyOptions options, ILogger logger)
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(
                    retryCount: options.RetryCount,
                    sleepDurationProvider: attempt =>
                        TimeSpan.FromSeconds(Math.Pow(options.InitialBackoffSeconds, attempt)),
                    onRetry: (outcome, delay, attempt, _) =>
                    {
                        logger.LogWarning($"Retry {attempt} after {delay.TotalSeconds}s due to: {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()}");
                    });
        }

        /// <summary>
        /// Add Circuit Breaker policy to the HTTP client.
        /// 2 attempts before breaking the circuit for 30 seconds(Default values).
        /// </summary>
        /// <returns></returns>
        private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(ExchangeProviderResiliencyOptions options, ILogger logger)
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: options.CircuitBreakerFailureThreshold,
                    durationOfBreak: TimeSpan.FromSeconds(options.CircuitBreakerDurationInSeconds),
                    onBreak: (outcome, delay) =>
                        logger.LogWarning($"Circuit opened for {delay.TotalSeconds}s due to: {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()}"),
                    onReset: () =>
                        logger.LogWarning("Circuit closed."),
                    onHalfOpen: () =>
                        logger.LogWarning("Circuit is half-open, testing...")
                );
        }
    }
}