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
    }
}
