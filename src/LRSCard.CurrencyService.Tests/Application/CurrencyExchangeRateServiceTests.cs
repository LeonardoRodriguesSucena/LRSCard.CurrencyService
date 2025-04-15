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
            var options = Options.Create(new CurrencyRulesOptions
            {
                BlockedCurrencyCodes = new List<string> { "MXN" }
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
            // Arrange
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

            // Act
            // Call the actual method under test
            var result = await _service.GetExchangeRate(request);

            // Assert

            // Check that the result is not null
            Assert.NotNull(result);
            // Verify the base currency in the response matches expectation
            Assert.Equal("USD", result.Base);
            // Verify that the number of rates returned matches what's in the mock result
            Assert.Equal(expected.Rates.Count, result.Rates.Count);
        }

        [Fact] // Marks this method as a unit test
        public async Task GetCurrencyConvertion_FiltersBlockedCurrencies()
        {
            // Arrange
            // Create a conversion request for 100 USD to EUR and MXN
            var request = new GetCurrencyConversionRequest
            {
                Amount = 100,
                BaseCurrency = "USD",
                Symbols = new List<string> { "EUR", "MXN" } // MXN is expected to be filtered
            };

            // Simulate the external exchange provider returning rates for both EUR and MXN
            var rates = new CurrencyRates
            {
                Amount = 100,
                Base = "USD",
                Rates = new Dictionary<string, float>
                {
                    { "EUR", 0.85f },  // Acceptable currency
                    { "MXN", 0.002f }  // Blocked currency
                }
            };

            // Configure the mock to return the above rates when the method is called
            _exchangeRateProviderMock
                .Setup(x => x.GetExchangeRate(100, null, "USD", request.Symbols))
                .ReturnsAsync(rates);

            // Act

            // Call the actual service method
            var result = await _service.GetCurrencyConvertion(request);

            // Assert

            // Assert that only one currency remains in the filtered result
            Assert.Single(result.Rates);

            // Assert that MXN has been filtered out from the response
            Assert.DoesNotContain("MXN", result.Rates.Keys);
        }

        [Fact] 
        public async Task GetHistoricalExchangeRatePaginated_ReturnsExpectedData()
        {
            // Arrange

            // Create a request for historical exchange rates from Jan 1 to Jan 3, 2024
            // with pagination set to return all 3 days on page 1
            var request = new GetHistoricalExchangeRateRequest
            {
                BaseCurrency = "USD",
                InitialDate = new DateTime(2024, 1, 1),
                EndDate = new DateTime(2024, 1, 3),
                Pagination = new Application.Common.Pagination { Page = 1, PageSize = 3 }
            };

            // Mocked currency rate to be returned for each day
            var rate = new CurrencyRates
            {
                Base = "USD",
                Amount = 1,
                Rates = new Dictionary<string, float> { { "EUR", 0.85f } }
            };

            // Setup the exchange rate provider to always return the same `rate` object
            // regardless of the specific date provided (simplified for the test)
            _exchangeRateProviderMock
                .Setup(x => x.GetExchangeRate(1, new DateTime(), "USD", null))
                .ReturnsAsync(rate);

            // Setup the cache mock to return null for all dates,
            // forcing the service to call the exchange rate provider
            _currencyRateCacheMock
                .Setup(x => x.GetAsync(It.IsAny<DateTime>(), "USD"))
                .ReturnsAsync((CurrencyRates)null);

            // Act

            // Call the method under test
            var result = await _service.GetHistoricalExchangeRatePaginated(request);

            // Assert

            // Verify that the result contains exactly 3 rate entries (1 for each day)
            Assert.Equal(3, result.Items.Count);

            // Verify that the total count of date entries is correct (Jan 1 to Jan 3 = 3 days)
            Assert.Equal(3, result.TotalCount);
        }

    }
}
