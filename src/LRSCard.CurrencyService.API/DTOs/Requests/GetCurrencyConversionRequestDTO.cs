using System.ComponentModel.DataAnnotations;
using LRSCard.CurrencyService.API.Validations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;

namespace LRSCard.CurrencyService.API.DTOs.Requests
{
    public class GetCurrencyConversionRequestDTO : IValidatableObject
    {
        [Required]
        public float Amount { get; set; }

        [Required]
        public string BaseCurrency { get; set; }
        public List<string> DestinationCurrencies { get; set; } = new List<string>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            //check if the currency code is valid and is not blocked
            if (Amount <= 0)
            {
                yield return new ValidationResult($"amount is not valid. It should be greater then 0.", new[] { nameof(BaseCurrency) });
            }

            //check if the currency code is valid and is not blocked
            if (!CurrencyCodeValidator.IsValidCurrencyCode(BaseCurrency) || CurrencyCodeValidator.IsBlocked(BaseCurrency))
            {
                yield return new ValidationResult($"baseCurrency '{BaseCurrency}' is not valid or not supported.", new[] { nameof(BaseCurrency) });
            }

            //check if the currency code is valid and not a blocked currency
            var invalids = DestinationCurrencies.Where(
                                    c => CurrencyCodeValidator.IsBlocked(c.ToUpperInvariant()) || 
                                         !CurrencyCodeValidator.IsValidCurrencyCode(c.ToUpperInvariant())
                                                ).ToList();

            if (invalids.Any())
            {
                yield return new ValidationResult($"The following currencies are not supported: {string.Join(", ", invalids)}", new[] { nameof(DestinationCurrencies) });
            }

            //Nothing to convert
            if(DestinationCurrencies.Count() == 1 && DestinationCurrencies.First().ToUpper() == BaseCurrency.ToUpper())
                yield return new ValidationResult($"baseCurrency is equal destinationCurrencies. Nothing to convert.", new[] { nameof(DestinationCurrencies) });


            //just uppercasing the currency codes
            BaseCurrency = BaseCurrency.ToUpperInvariant();
            DestinationCurrencies = DestinationCurrencies.Select(c => c.ToUpperInvariant()).ToList();
        }
    }
}
