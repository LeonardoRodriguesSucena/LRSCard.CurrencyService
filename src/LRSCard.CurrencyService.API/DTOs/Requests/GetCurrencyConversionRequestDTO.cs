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
        public float Amount { get; set; }
        /// <summary>
        /// The base currency (USD, EUR,...)
        /// </summary>
        public string BaseCurrency { get; set; }
        /// <summary>
        /// Destinations currencies
        /// </summary>
        public List<string> DestinationCurrencies { get; set; } = new List<string>();
    }
}
