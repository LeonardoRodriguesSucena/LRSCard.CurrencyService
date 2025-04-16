using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using LRSCard.CurrencyService.Application;
using LRSCard.CurrencyService.Application.Interfaces;
using LRSCard.CurrencyService.Application.Options;
using LRSCard.CurrencyService.Application.Requests;
using LRSCard.CurrencyService.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Castle.Components.DictionaryAdapter.Xml;

namespace LRSCard.CurrencyService.Tests
{
    public class CurrencyExchangeRateServiceTests
    {
        private readonly Mock<IExchangeRateProvider> _exchangeRateProviderMock = new();
        private readonly Mock<ICurrencyRateCache> _currencyRateCacheMock = new();
        private readonly Mock<ILogger<CurrencyExchangeRateService>> _loggerMock = new();
        private readonly CurrencyExchangeRateService _service;

        public CurrencyExchangeRateServiceTests()
        {
            //Initializing the service with mocked values to be able to apply the rules
            //I will not get them from config, because I want them here to let the rule explicitly in this test class
            var options = Options.Create(new CurrencyRulesOptions
            {
                ValidCurrencyCodes = new List<string> {"USD", "GBP", "AUD", "BGN", "BRL", "EUR", "TRY", "PLN", "THB", "MXN" },
                BlockedCurrencyCodes = new List<string> { "TRY", "PLN", "THB", "MXN" },
            });

            _service = new CurrencyExchangeRateService(
                _exchangeRateProviderMock.Object,
                _currencyRateCacheMock.Object,
                options,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task GetExchangeRate_ReturnsExpectedResult()
        {
            // ------------------ Arrange ------------------
            // Creates a request object that simulates a request for exchange rates
            var request = new GetExchangeRateRequest
            {
                Amount = 1,
                BaseCurrency = "USD",
                Date = new DateTime(2024, 1, 1),
                Symbols = new List<string> { "EUR", "GBP" }
            };

            // Simulated result returned by the mocked provider
            var expected = new CurrencyRates
            {
                Amount = 1,
                Base = "USD",
                Date = request.Date.Value,
                Rates = new Dictionary<string, float> { { "EUR", 0.61f }, { "GBP", 0.62f } }
            };

            // Mock the _exchangeRateProvider behavior:
            // When GetExchangeRate is called with these specific arguments, return `expected`
            _exchangeRateProviderMock
                .Setup(x => x.GetExchangeRate(1, request.Date, "USD", request.Symbols))
                .ReturnsAsync(expected);

            // ------------------ Act ------------------
            // Call the actual method under test
            var result = await _service.GetExchangeRate(request);

            // ------------------ Assert ------------------

            // Check that the result is not null
            Assert.NotNull(result);
            // Verify the base currency in the response matches expectation
            Assert.Equal("USD", result.Base);
            // Verify that the number of rates returned matches what's in the mock result
            Assert.Equal(expected.Rates.Count, result.Rates.Count);
        }

        [Fact]
        public async Task GetCurrencyConvertion_FiltersBlockedCurrencies()
        {
            // ------------------ Arrange ------------------
            // Create a conversion request for 100 USD to EUR and MXN
            var request = new GetCurrencyConversionRequest
            {
                Amount = 100,
                BaseCurrency = "USD",
                Symbols = new List<string>(), // No input symbols, so it is expected in the result to get all currencies 
            };

            // Simulate the external exchange provider returning rates for both EUR and MXN
            var rates = new CurrencyRates
            {
                Amount = 100,
                Base = "USD",
                Rates = new Dictionary<string, float>
                {
                    { "EUR", 0.01f },  // Acceptable currency
                    { "CAD", 0.012f }, // Acceptable currency
                    { "MXN", 0.02f },  // Blocked currency
                    { "PLN", 0.03f },  // Blocked currency
                    { "THB", 0.04f }   // Blocked currency
                }
            };

            // Configure the mock to return the above rates when the method is called
            _exchangeRateProviderMock
                .Setup(x => x.GetExchangeRate(100, null, "USD", request.Symbols))
                .ReturnsAsync(rates);

            // ------------------ Act ------------------
            // Call the actual service method
            var result = await _service.GetCurrencyConvertion(request);

            // ------------------ Assert ------------------
            // Assert that only one currency remains in the filtered result
            Assert.Contains("EUR", result.Rates.Keys);
            Assert.Contains("CAD", result.Rates.Keys);

            // Assert that MXN,PLN,THB has been filtered out from the response
            Assert.DoesNotContain("MXN", result.Rates.Keys);
            Assert.DoesNotContain("PLN", result.Rates.Keys);
            Assert.DoesNotContain("THB", result.Rates.Keys);
        }

        [Fact]
        public async Task GetHistoricalExchangeRatePaginated_ReturnsExpectedData()
        {
            // ------------------ Arrange ------------------
            var request = new GetHistoricalExchangeRateRequest
            {
                BaseCurrency = "USD",
                InitialDate = new DateTime(2024, 1, 1),
                EndDate = new DateTime(2024, 1, 3),
                Pagination = new Application.Common.Pagination { Page = 1, PageSize = 3 }
            };

            var rate = new CurrencyRates
            {
                Base = "USD",
                Amount = 1,
                Rates = new Dictionary<string, float> { { "EUR", 0.85f } }
            };

            //setup the _exchangeRateProvider to return the rate for each date
            _exchangeRateProviderMock.Setup(x => x.GetExchangeRate(
                    It.IsAny<float?>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<string?>(),
                    It.IsAny<List<string>?>()))
                .ReturnsAsync((float? amount, DateTime? date, string? baseCurrency, List<string>? symbols) =>
                {
                    return new CurrencyRates
                    {
                        Base = baseCurrency ?? "USD",
                        Amount = amount ?? 1,
                        Date = date ?? DateTime.UtcNow,
                        Rates = new Dictionary<string, float>
                        {
                            { "EUR", 0.85f + (float)((date?.Day ?? 0) * 0.001f) } 
                        }
                    };
                });

            //setup the cache to return null, to simulate that the data is not in the cache forcing the service to get it from the provider
            _currencyRateCacheMock
                .Setup(x => x.GetAsync(It.IsAny<DateTime>(), "USD"))
                .ReturnsAsync((CurrencyRates) null);

            // ------------------ Act ------------------
            var result = await _service.GetHistoricalExchangeRatePaginated(request);

            // ------------------ Assert ------------------
            Assert.Equal(3, result.Items.Count);
            Assert.Equal(3, result.TotalCount);
        }


    }
}
