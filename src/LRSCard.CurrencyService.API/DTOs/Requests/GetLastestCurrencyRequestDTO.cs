using System.ComponentModel.DataAnnotations;
using LRSCard.CurrencyService.API.Validations;

namespace LRSCard.CurrencyService.API.DTOs.Requests
{
    public class GetLastestCurrencyRequestDTO: IValidatableObject
    {
        [Required]
        public string BaseCurrency { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            //check if the currency code is valid
            if (!CurrencyCodeValidator.IsValidCurrencyCode(BaseCurrency))
            {
                yield return new ValidationResult($"BaseCurrency '{BaseCurrency}' is not valid or not supported.", new[] { nameof(BaseCurrency) });
            }

            //just uppercasing the currency codes
            BaseCurrency = BaseCurrency.ToUpperInvariant();
            
        }
    }
}
