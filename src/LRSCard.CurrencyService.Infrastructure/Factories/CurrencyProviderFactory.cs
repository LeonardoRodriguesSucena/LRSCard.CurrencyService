using LRSCard.CurrencyService.Application.Common;
using LRSCard.CurrencyService.Application.Interfaces;
using LRSCard.CurrencyService.Infrastructure.ExchangeRateProviders.Frankfurter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LRSCard.CurrencyService.Infrastructure.Factories
{
    public class CurrencyProviderFactory : ICurrencyProviderFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public CurrencyProviderFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IExchangeRateProvider GetProvider(CurrencyProviderType type)
        {
            switch (type)
            {
                case CurrencyProviderType.Frankfurter:
                    return _serviceProvider.GetRequiredService<FrankfurterExchangeRateProvider>();

                default:
                    throw new NotImplementedException($"Currency provider '{type}' is not supported.");
            };

        }
    }
}
