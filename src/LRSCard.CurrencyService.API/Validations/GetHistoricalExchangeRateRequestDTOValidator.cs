using FluentValidation;
using LRSCard.CurrencyService.API.DTOs.Requests;

namespace LRSCard.CurrencyService.API.Validations
{
    public class GetHistoricalExchangeRateRequestDTOValidator : AbstractValidator<GetHistoricalExchangeRateRequestDTO>
    {
        public GetHistoricalExchangeRateRequestDTOValidator()
        {
            RuleFor(x => x.BaseCurrency)
                .NotEmpty().WithMessage("baseCurrency is required.")
                .Must(x => CurrencyCodeValidator.IsValidCurrencyCode(x))
                .WithMessage(x => $"baseCurrency '{x.BaseCurrency}' is not valid or not supported.");                

            RuleFor(x => x.InitialDate)
                .NotEmpty().WithMessage("initialDate is required.");

            RuleFor(x => x.EndDate)
                .NotEmpty().WithMessage("endDate is required.")
                .GreaterThanOrEqualTo(x => x.InitialDate)
                .WithMessage("endDate must be greater or equal to initialDate.");

            RuleFor(x => x.Page)
                .GreaterThan(0).WithMessage("page must be greater than 0.");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 60).WithMessage("pageSize must be between 1 and 60.");
        }
    }
}
