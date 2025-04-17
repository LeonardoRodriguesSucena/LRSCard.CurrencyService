using System.ComponentModel.DataAnnotations;
using LRSCard.CurrencyService.API.Validations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;

namespace LRSCard.CurrencyService.API.DTOs.Requests
{
    public class GetCurrencyConversionRequestDTO
    {
        /// <summary>
        /// Amount of baseCurrency
        /// </summary>
        /// <example>1</example>
        public float Amount { get; set; }

        /// <summary>
        /// The base currency (USD, EUR,...)
        /// </summary>
        /// <example>USD</example>
        public string BaseCurrency { get; set; }

        /// <summary>
        /// Destinations currencies
        /// </summary>
        /// <example>["EUR","CAD"]</example>
        public List<string> DestinationCurrencies { get; set; } = new List<string>();

        /// <summary>
        /// Currency provider
        /// </summary>
        /// <example>Frankfurter</example>
        [FromQuery(Name = "provider")]
        public string? Provider { get; set; }
    }
}
