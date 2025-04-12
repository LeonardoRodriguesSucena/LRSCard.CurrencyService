using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using LRSCard.CurrencyService.Application.Interfaces;
using LRSCard.CurrencyService.Application.Options;

namespace LRSCard.CurrencyService.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<CurrencyRulesOptions>(configuration.GetSection("CurrencyRules"));

            services.AddScoped<ICurrencyExchangeRateService, CurrencyExchangeRateService>();

            return services;
        }
    }
}