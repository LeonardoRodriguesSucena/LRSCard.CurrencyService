using FluentValidation;
using LRSCard.CurrencyService.API.DTOs.Requests;

namespace LRSCard.CurrencyService.API.Validations
{
    public class GetLastestCurrencyRequestDTOValidator : AbstractValidator<GetLastestCurrencyRequestDTO>
    {
        public GetLastestCurrencyRequestDTOValidator()
        {
            RuleFor(x => x.BaseCurrency)
                .NotEmpty().WithMessage("baseCurrency is required.")
                .Must(x => CurrencyCodeValidator.IsValidCurrencyCode(x))
                .WithMessage(x => $"baseCurrency '{x.BaseCurrency}' is not valid or not supported.");            
        }
    }
}