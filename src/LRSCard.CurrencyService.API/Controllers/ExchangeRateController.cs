using LRSCard.CurrencyService.API.DTOs.Requests;
using LRSCard.CurrencyService.API.DTOs.Responses;
using LRSCard.CurrencyService.Application.Interfaces;
using LRSCard.CurrencyService.Application.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LRSCard.CurrencyService.API.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/exchange-rates")]
    [ApiController]    
    public class ExchangeRateController : ControllerBase
    {
        private readonly ICurrencyExchangeRateService _currencyExchangeRateService;

        public ExchangeRateController(ICurrencyExchangeRateService currencyExchangeRateService)
        {
            _currencyExchangeRateService = currencyExchangeRateService;
        }

        [HttpGet("latest")]
        public async Task<IActionResult> GetLatest([FromQuery]GetLastestCurrencyRequestDTO request)
        {
            var response = await _currencyExchangeRateService.GetExchangeRate(new GetExchangeRateRequest{ BaseCurrency = request.BaseCurrency});
            CurrencyRatesDTO dto = new CurrencyRatesDTO
            {
                Amount = response.Amount,
                BaseCurrency = response.Base,
                Date = response.Date,
                TargetCurrencies = response.Rates
            };

            return Ok(dto);
        }

        [HttpPost("conversion")]
        public async Task<IActionResult> GetCurrencyConversion([FromBody] GetCurrencyConversionRequestDTO request)
        {
            var serviceRequest = new GetCurrencyConversionRequest 
            { 
                Amount = request.Amount, 
                BaseCurrency = request.BaseCurrency, 
                Symbols = request.DestinationCurrencies
            };
            var response = await _currencyExchangeRateService.GetCurrencyConvertion(serviceRequest);

            CurrencyRatesDTO dto = new CurrencyRatesDTO
            {
                Amount = response.Amount,
                BaseCurrency = response.Base,
                Date = response.Date,
                TargetCurrencies = response.Rates
            };

            return Ok(dto);
        }


    }
}
