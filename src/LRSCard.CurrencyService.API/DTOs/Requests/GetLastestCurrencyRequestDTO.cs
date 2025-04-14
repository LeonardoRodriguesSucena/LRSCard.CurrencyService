using Microsoft.AspNetCore.Mvc;

namespace LRSCard.CurrencyService.API.DTOs.Requests
{
    public class GetLastestCurrencyRequestDTO
    {
        /// <summary>
        /// The base currency (USD, EUR,...)
        /// </summary>
        [FromQuery(Name = "baseCurrency")]
        public string BaseCurrency { get; set; }
    }
}
