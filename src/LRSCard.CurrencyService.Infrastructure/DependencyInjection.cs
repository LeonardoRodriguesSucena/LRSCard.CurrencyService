using Microsoft.Extensions.Http;
using LRSCard.CurrencyService.Application.Interfaces;
using LRSCard.CurrencyService.Infrastructure.ExchangeRateProviders.Frankfurter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LRSCard.CurrencyService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var baseUrl = configuration["ExchangeProvider:Frankfurter:BaseUrl"] ?? "https://api.frankfurter.app";

            services.AddHttpClient<IExchangeRateProvider, FrankfurterExchangeRateProvider>((provider, client) =>
            {
                client.BaseAddress = new Uri(baseUrl);
            });

            return services;
        }
    }
}