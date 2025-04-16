using LRSCard.CurrencyService.Application.Common;

namespace LRSCard.CurrencyService.Application.Interfaces
{
    public interface ICurrencyProviderFactory
    {
        IExchangeRateProvider GetProvider(CurrencyProviderType type);
    }
}
