using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using LRSCard.CurrencyService.API.Validations;

namespace LRSCard.CurrencyService.API
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
        {
            CurrencyCodeValidator.Initialize(configuration);
            return services;
        }
    }
}
