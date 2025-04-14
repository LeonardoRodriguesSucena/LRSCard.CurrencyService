using FluentValidation;
using LRSCard.CurrencyService.API.DTOs.Requests;

namespace LRSCard.CurrencyService.API.Validations
{
    public class AuthRequestDTOValidator : AbstractValidator<AuthRequestDTO>
    {
        public AuthRequestDTOValidator()
        {
            RuleFor(x => x.Login).NotEmpty().WithMessage("login is required.");
            RuleFor(x => x.Password).NotEmpty().WithMessage("password is required.");

        }
    }
}