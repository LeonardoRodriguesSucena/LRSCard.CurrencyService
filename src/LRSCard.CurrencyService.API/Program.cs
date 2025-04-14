using LRSCard.CurrencyService.Application;
using LRSCard.CurrencyService.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Serilog;
using LRSCard.CurrencyService.API.Middlewares;


namespace LRSCard.CurrencyService.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try {

                var builder = WebApplication.CreateBuilder(args);

                Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .CreateLogger();

                Log.Information("Starting...");

                
                builder.Host.UseSerilog();

                builder.Services.AddApplication(builder.Configuration);
                builder.Services.AddInfrastructure(builder.Configuration);
                builder.Services.AddPresentation(builder.Configuration);


                builder.Services.AddControllers();
                //builder.Services.AddControllers().AddJsonOptions(options =>
                //{
                //turn off camel case in respose, so it will same as response dto
                //options.JsonSerializerOptions.PropertyNamingPolicy = null;
                //}); 

                // API version options
                builder.Services.AddApiVersioning(options =>
                {
                    options.AssumeDefaultVersionWhenUnspecified = true;
                    options.DefaultApiVersion = new ApiVersion(1, 0);
                    options.ReportApiVersions = true;
                });
                builder.Services.AddVersionedApiExplorer(options =>
                {
                    options.GroupNameFormat = "'v'V";
                    options.SubstituteApiVersionInUrl = true;
                });

                // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                builder.Services.AddEndpointsApiExplorer();

                builder.Services.AddSwaggerGen(options =>
                {
                    options.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Version = "v1",
                        Title = "Currency Service API",
                        Description = "API for currency rate operations"
                    });

                    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                    options.IncludeXmlComments(xmlPath);

                    //JWT Auth support
                    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Description = "Enter your JWT token in the format: Bearer {your token}"
                    });

                    options.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Id = "Bearer",
                                    Type = ReferenceType.SecurityScheme
                                }
                            },
                            new List<string>()
                        }
                    });


                });

                var app = builder.Build();

                // Configure the HTTP request pipeline.
                if (!app.Environment.IsProduction())
                {
                    var apiVersionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
                    app.UseSwagger();
                    app.UseSwaggerUI(options =>
                    {
                        foreach (var description in apiVersionProvider.ApiVersionDescriptions)
                        {
                            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", $"Version {description.ApiVersion}");
                        }
                    });
                }

                app.UseHttpsRedirection();

                

                app.UseRateLimiter();
                app.UseAuthentication();
                app.UseAuthorization();

                app.UseMiddleware<RequestLoggingMiddleware>();

                app.MapControllers();

                Log.Information("Currency Service API is now running...");
                app.Run();

            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application failed to start correctly.");
            }
            finally
            {
                Log.CloseAndFlush();
            }

            
        }
    }
}
