using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using LRSCard.CurrencyService.API;
using LRSCard.CurrencyService.API.DTOs.Requests;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace LRSCard.CurrencyService.Tests.API
{
    public class ExchangeRatesIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public ExchangeRatesIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task<string> GetAccessTokenAsync()
        {
            var loginPayload = new
            {
                login = "leonardo.sucena",
                password = "pwd123"
            };

            var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginPayload);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);
            return json.RootElement.GetProperty("access_token").GetString()!;
        }

        [Fact]
        public async Task GetLatestExchangeRates_ReturnsOk()
        {
            // Arrange
            var token = await GetAccessTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var baseCurrency = "USD";

            // Act
            var response = await _client.GetAsync($"/api/v1/exchange-rates/latest?baseCurrency={baseCurrency}");

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task GetByPeriod_ReturnsOk()
        {
            // Arrange
            var token = await GetAccessTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            string baseCurrency = "USD";
            string initialDate = "2025-01-01";
            string endDate = "2025-01-30";

            // Act
            var response = await _client.GetAsync($"/api/v1/exchange-rates/history?baseCurrency={baseCurrency}&initialDate={initialDate}&endDate={endDate}");

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task PostCurrencyConversion_ReturnsOk()
        {
            // Arrange
            var token = await GetAccessTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var request = new GetCurrencyConversionRequestDTO
            {
                Amount = 100,
                BaseCurrency = "USD",
                DestinationCurrencies = new List<string> { "EUR", "GBP" }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/exchange-rates/convert", request);

            // Assert
            response.EnsureSuccessStatusCode(); // Verifies 200 OK
        }

        [Fact]
        public async Task PostCurrencyConversion_BlockedBaseCurrency_ReturnsBadRequest()
        {
            // Arrange
            var token = await GetAccessTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // MXN is blocked, so we should receive a bad request
            var request = new GetCurrencyConversionRequestDTO
            {
                Amount = 100,
                BaseCurrency = "USD",
                DestinationCurrencies = new List<string> { "EUR", "GBP", "MXN" } 
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/exchange-rates/convert", request);

            // Assert
            Assert.True(response.StatusCode == System.Net.HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task PostCurrencyConversion_BlockedDestinationCurrencies_ReturnsBadRequest()
        {
            // Arrange
            var token = await GetAccessTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // MXN is blocked, so we should receive a bad request
            var request = new GetCurrencyConversionRequestDTO
            {
                Amount = 100,
                BaseCurrency = "MXN",
                DestinationCurrencies = new List<string> { "EUR", "GBP", "USD" }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/exchange-rates/convert", request);

            // Assert
            Assert.True(response.StatusCode == System.Net.HttpStatusCode.BadRequest);
        }
    }
}
