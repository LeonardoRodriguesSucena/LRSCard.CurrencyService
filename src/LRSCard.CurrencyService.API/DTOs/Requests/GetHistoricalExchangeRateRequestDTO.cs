using System.ComponentModel.DataAnnotations;
using LRSCard.CurrencyService.API.DTOs.Common;
using LRSCard.CurrencyService.API.Validations;
using Microsoft.AspNetCore.Components.Forms;

namespace LRSCard.CurrencyService.API.DTOs.Requests
{
    public class GetHistoricalExchangeRateRequestDTO: IValidatableObject
    {
        /// <summary>
        /// The base currency (e.g., USD, EUR)
        /// </summary>
        [Required]
        public string BaseCurrency { get; set; }

        /// <summary>
        /// Start of the date range (format: yyyy-MM-dd)
        /// </summary>
        [Required]
        [DataType(DataType.Date)]
        public DateTime InitialDate { get; set; }

        /// <summary>
        /// End of the date range (format: yyyy-MM-dd)
        /// </summary>
        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Page number (must be greater than 0)
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// Page size (between 1 and 60)
        /// </summary>
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
