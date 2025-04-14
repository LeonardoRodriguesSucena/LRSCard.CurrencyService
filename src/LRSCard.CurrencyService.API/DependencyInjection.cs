using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using FluentValidation.AspNetCore;
using LRSCard.CurrencyService.API.Validations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using LRSCard.CurrencyService.API.Options;
using System.Threading.RateLimiting;

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

            // Rate Limiting
            services.Configure<ApiRateLimitOptions>(configuration.GetSection("APIRateLimit"));
            var rateLimitOptions = configuration.GetSection("APIRateLimit").Get<ApiRateLimitOptions>() ?? new();

            services.AddRateLimiter(options =>
            {
                //.Net8 by default uses RejectionStatusCode = 503(ServiceUnavailable) for rate limiting
                // I am changing it to 429(TooManyRequests) which is more common
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                // Default rate limit policy, for authenticated users, for example
                options.AddPolicy("default", context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.Connection?.RemoteIpAddress?.ToString() ?? "default",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = rateLimitOptions.Default.PermitLimit,
                            Window = TimeSpan.FromSeconds(rateLimitOptions.Default.WindowSeconds),
                            QueueLimit = rateLimitOptions.Default.QueueLimit,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        }));

                // Anonimous rate limit policy, more restrictive
                options.AddPolicy("anonymous", context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.Connection?.RemoteIpAddress?.ToString() ?? "anonymous",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = rateLimitOptions.Anonymous.PermitLimit,
                            Window = TimeSpan.FromSeconds(rateLimitOptions.Anonymous.WindowSeconds),
                            QueueLimit = rateLimitOptions.Anonymous.QueueLimit,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                        }));
            });


            return services;
        }
    }
}
