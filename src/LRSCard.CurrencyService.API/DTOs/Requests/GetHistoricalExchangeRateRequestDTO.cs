using System.ComponentModel.DataAnnotations;
using LRSCard.CurrencyService.API.DTOs.Common;
using LRSCard.CurrencyService.API.Validations;
using Microsoft.AspNetCore.Components.Forms;

namespace LRSCard.CurrencyService.API.DTOs.Requests
{
    public class GetHistoricalExchangeRateRequestDTO: IValidatableObject
    {
        [Required]
        public string BaseCurrency { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime InitialDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        public PaginationDTO Pagination { get; set; } = new PaginationDTO();


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
