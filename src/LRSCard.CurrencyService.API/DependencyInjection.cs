using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using FluentValidation.AspNetCore;
using LRSCard.CurrencyService.API.Validations;

namespace LRSCard.CurrencyService.API
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
        {
            CurrencyCodeValidator.Initialize(configuration);

            //Dto Validations
            services.AddValidatorsFromAssemblyContaining<GetLastestCurrencyRequestDTOValidator>();
            services.AddFluentValidationAutoValidation();

            

            return services;
        }
    }
}
