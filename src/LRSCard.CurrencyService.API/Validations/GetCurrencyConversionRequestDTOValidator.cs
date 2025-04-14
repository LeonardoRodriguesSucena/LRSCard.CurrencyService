using FluentValidation;
using LRSCard.CurrencyService.API.DTOs.Requests;

namespace LRSCard.CurrencyService.API.Validations 
{
    public class GetCurrencyConversionRequestDTOValidator : AbstractValidator<GetCurrencyConversionRequestDTO>
    {
        public GetCurrencyConversionRequestDTOValidator()
        {
            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("amount must be greater than 0.");

            RuleFor(x => x.BaseCurrency)
                .NotEmpty().WithMessage("baseCurrency is required.")
                .Must(code => CurrencyCodeValidator.IsValidCurrencyCode(code) && !CurrencyCodeValidator.IsBlocked(code))
                .WithMessage(code => $"baseCurrency '{code.BaseCurrency}' is not valid or not supported.");

            RuleFor(x => x.DestinationCurrencies)
                .Must(currencies =>
                {
                    var invalids = currencies.Where(c => CurrencyCodeValidator.IsBlocked(c.ToUpperInvariant()) ||
                                                         !CurrencyCodeValidator.IsValidCurrencyCode(c.ToUpperInvariant()))
                                             .ToList();
                    return !invalids.Any();
                })
                .WithMessage("One or more destination currencies are not supported.");

            RuleFor(x => x)
                .Must(x => !(x.DestinationCurrencies.Count == 1 &&
                             x.DestinationCurrencies.First().ToUpperInvariant() == x.BaseCurrency.ToUpperInvariant()))
                .WithMessage("baseCurrency is equal to destinationCurrencies. Nothing to convert.");
        }
    }
}

