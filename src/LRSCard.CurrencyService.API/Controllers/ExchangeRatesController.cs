using LRSCard.CurrencyService.API.DTOs.Common;
using LRSCard.CurrencyService.API.DTOs.Requests;
using LRSCard.CurrencyService.API.DTOs.Responses;
using LRSCard.CurrencyService.Application.Common;
using LRSCard.CurrencyService.Application.Interfaces;
using LRSCard.CurrencyService.Application.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace LRSCard.CurrencyService.API.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/exchange-rates")]
    [ApiController]
    [Authorize(Roles = "admin")]
    [EnableRateLimiting("default")]
    public class ExchangeRatesController : ControllerBase
    {
        private readonly ICurrencyExchangeRateService _currencyExchangeRateService;
        private ILogger<ExchangeRatesController> _logger;

        public ExchangeRatesController(ICurrencyExchangeRateService currencyExchangeRateService, ILogger<ExchangeRatesController> logger)
        {
            _currencyExchangeRateService = currencyExchangeRateService;
            _logger = logger;
        }

        [HttpGet("latest")]
        [ProducesResponseType(typeof(CurrencyRatesDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetLatest([FromQuery] GetLastestCurrencyRequestDTO request)
        {
            try {
                var response = await _currencyExchangeRateService.GetExchangeRate(
                    new GetExchangeRateRequest { 
                           BaseCurrency = request.BaseCurrency,
                           CurrencyProvider = mapToCurrencyProviderType(request.Provider)
                    });

                CurrencyRatesDTO dto = new CurrencyRatesDTO
                {
                    Amount = response.Amount,
                    BaseCurrency = response.Base,
                    Date = response.Date,
                    TargetCurrencies = response.Rates
                };

                return Ok(dto);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error in GetLatest operation");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.Please try again later.");
            }


        }

        [HttpGet("history")]
        [ProducesResponseType(typeof(PaginationResultDTO<CurrencyRatesDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByPeriod([FromQuery] GetHistoricalExchangeRateRequestDTO request)
        {
            try
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
                    },
                    CurrencyProvider = mapToCurrencyProviderType(request.Provider)
                };

                var serviceResponse = await _currencyExchangeRateService.GetHistoricalExchangeRatePaginated(serviceRequest);

                var dto = new LRSCard.CurrencyService.API.DTOs.Common.PaginationResultDTO<CurrencyRatesDTO>
                {
                    TotalCount = serviceResponse.TotalCount,
                    Page = serviceResponse.Page,
                    PageSize = serviceResponse.PageSize,
                    Items = serviceResponse.Items.Select(x => new CurrencyRatesDTO
                    {
                        Amount = x.Amount,
                        BaseCurrency = x.Base,
                        Date = x.Date,
                        TargetCurrencies = x.Rates
                    }).ToList()
                };

                return Ok(dto);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByPeriod operation");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.Please try again later.");
            }            
        }

        [HttpPost("convert")]
        [ProducesResponseType(typeof(CurrencyRatesDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCurrencyConversion([FromBody] GetCurrencyConversionRequestDTO request)
        {
            try
            {
                var serviceRequest = new GetCurrencyConversionRequest
                {
                    Amount = request.Amount,
                    BaseCurrency = request.BaseCurrency,
                    Symbols = request.DestinationCurrencies,
                    CurrencyProvider = mapToCurrencyProviderType(request.Provider)
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
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error in GetCurrencyConversion operation");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.Please try again later.");
            }            
        }

        private CurrencyProviderType mapToCurrencyProviderType(string? provider)
        {
            //map the provider received in the DTO to the enum
            //For now we just have 1 provider Frankfurter, but like this it is prepated for future providers

            if (!string.IsNullOrEmpty(provider) && 
                Enum.TryParse(provider, true, out CurrencyProviderType providerType)
               )
                return providerType;

            return CurrencyProviderType.Frankfurter;

        }

    }
}
