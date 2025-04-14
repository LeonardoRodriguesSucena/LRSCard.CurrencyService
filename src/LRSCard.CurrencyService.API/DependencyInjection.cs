using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using FluentValidation.AspNetCore;
using LRSCard.CurrencyService.API.Validations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using LRSCard.CurrencyService.API.Options;

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

            //JWT Authentication
            services.Configure<JwtSettings>(configuration.GetSection("IdentityProvider:JwtSettings"));
            var jwtSettings = configuration.GetSection("IdentityProvider:JwtSettings").Get<JwtSettings>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
                    };
                });


            return services;
        }
    }
}
