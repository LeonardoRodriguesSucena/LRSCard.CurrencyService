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

namespace LRSCard.CurrencyService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var baseUrl = configuration["ExchangeProvider:Frankfurter:BaseUrl"] ?? "https://api.frankfurter.app";

            var resiliencyConfig = configuration
                .GetSection("ExchangeProviderResiliency")
                .Get<ExchangeProviderResiliencyOptions>() ?? new ExchangeProviderResiliencyOptions();

            services.AddHttpClient<IExchangeRateProvider, FrankfurterExchangeRateProvider>((provider, client) =>
            {
                client.BaseAddress = new Uri(baseUrl);
            })
            .AddPolicyHandler(GetRetryPolicy(resiliencyConfig))
            .AddPolicyHandler(GetCircuitBreakerPolicy(resiliencyConfig));

            
            //Redis
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration["Redis:ConnectionString"] ?? "localhost:6379";
            });
            services.AddSingleton<ICurrencyRateCache, RedisCurrencyRateCache>();

            return services;
        }

        /// This method configures a retry policy using Polly for transient HTTP errors.
        /// It retries the request up to 3 times with an exponential backoff strategy.(Default values)
        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(ExchangeProviderResiliencyOptions options)
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(
                    retryCount: options.RetryCount,
                    sleepDurationProvider: attempt =>
                        TimeSpan.FromSeconds(Math.Pow(options.InitialBackoffSeconds, attempt)),
                    onRetry: (outcome, delay, attempt, _) =>
                    {
                        Console.WriteLine($"Retry {attempt} after {delay.TotalSeconds}s due to: {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()}");
                    });
        }

        /// <summary>
        /// Add Circuit Breaker policy to the HTTP client.
        /// 2 attempts before breaking the circuit for 30 seconds(Default values).
        /// </summary>
        /// <returns></returns>
        private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(ExchangeProviderResiliencyOptions options)
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: options.CircuitBreakerFailureThreshold,
                    durationOfBreak: TimeSpan.FromSeconds(options.CircuitBreakerDurationInSeconds),
                    onBreak: (outcome, delay) =>
                        Console.WriteLine($"Circuit opened for {delay.TotalSeconds}s due to: {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()}"),
                    onReset: () =>
                        Console.WriteLine("Circuit closed."),
                    onHalfOpen: () =>
                        Console.WriteLine("Circuit is half-open, testing...")
                );
        }
    }
}