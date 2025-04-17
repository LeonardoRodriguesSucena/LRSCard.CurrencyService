using Microsoft.AspNetCore.Mvc;

namespace LRSCard.CurrencyService.API.DTOs.Requests
{
    public class GetLastestCurrencyRequestDTO
    {
        /// <summary>
        /// The base currency (USD, EUR,...)
        /// </summary>
        /// <example>USD</example>
        [FromQuery(Name = "baseCurrency")]
        public string BaseCurrency { get; set; }

        /// <summary>
        /// Currency provider
        /// </summary>
        /// <example>Frankfurter</example>
        [FromQuery(Name = "provider")]
        public string? Provider { get; set; }
    }
}
