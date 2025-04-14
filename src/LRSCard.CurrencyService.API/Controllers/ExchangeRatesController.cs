using LRSCard.CurrencyService.API.DTOs.Common;
using LRSCard.CurrencyService.API.DTOs.Requests;
using LRSCard.CurrencyService.API.DTOs.Responses;
using LRSCard.CurrencyService.Application.Interfaces;
using LRSCard.CurrencyService.Application.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LRSCard.CurrencyService.API.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/exchange-rates")]
    [ApiController]    
    public class ExchangeRatesController : ControllerBase
    {
        private readonly ICurrencyExchangeRateService _currencyExchangeRateService;

        public ExchangeRatesController(ICurrencyExchangeRateService currencyExchangeRateService)
        {
            _currencyExchangeRateService = currencyExchangeRateService;
        }

        [Authorize(Roles = "admin")]
        [HttpGet("latest")]
        public async Task<IActionResult> GetLatest([FromQuery] GetLastestCurrencyRequestDTO request)
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

        [HttpGet("history")]
        public async Task<IActionResult> GetByPeriod([FromQuery] GetHistoricalExchangeRateRequestDTO request)
        {
            var serviceRequest = new GetHistoricalExchangeRateRequest
            {
                BaseCurrency = request.BaseCurrency,
                InitialDate = request.InitialDate,
                EndDate = request.EndDate,
                Pagination = new Application.Common.Pagination
                {
                    Page = request.Page,
                    PageSize = request.PageSize
                }
            };

            var serviceResponse = await _currencyExchangeRateService.GetHistoricalExchangeRatePaginated(serviceRequest);

            var dto = new LRSCard.CurrencyService.API.DTOs.Common.PaginationResultDTO<CurrencyRatesDTO>
            {
                TotalCount = serviceResponse.TotalCount,
                Page = serviceResponse.Page,
                PageSize = serviceResponse.PageSize,
                Items = serviceResponse.Items.Select(x => new CurrencyRatesDTO{
                        Amount = x.Amount,
                        BaseCurrency = x.Base,
                        Date = x.Date,
                        TargetCurrencies = x.Rates
                }).ToList()
            };

            return Ok(dto);
        }

        [HttpPost("convert")]
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
