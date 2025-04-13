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

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;


        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            //check if the currency code is valid
            if (!CurrencyCodeValidator.IsValidCurrencyCode(BaseCurrency))
                yield return new ValidationResult($"BaseCurrency '{BaseCurrency}' is not valid or not supported.", new[] { nameof(BaseCurrency) });
            
            //just uppercasing the currency codes
            BaseCurrency = BaseCurrency.ToUpperInvariant();

            //Pagination validations
            if (Page < 1)
                yield return new ValidationResult("Page must be greater than 0.", new[] { nameof(Page) });

            if (PageSize < 1 || PageSize > 60)
                yield return new ValidationResult("PageSize must be between 1 and 60.", new[] { nameof(PageSize) });

            if (InitialDate > EndDate)
                yield return new ValidationResult("InitialDate must be before or equal to EndDate.", new[] { nameof(InitialDate), nameof(EndDate) });


        }
    }
}
