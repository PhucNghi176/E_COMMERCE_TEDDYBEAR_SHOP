using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CONTRACT.CONTRACT.API.DependencyInjection.Extensions;
public static class ServiceCollectionExtensions
{
    public static void ConfigureCors(this IServiceCollection services)
    {
        _ = services.AddCors(options =>
        {
            // Get environment from service provider
            var serviceProvider = services.BuildServiceProvider();
            var environment = serviceProvider.GetRequiredService<IHostEnvironment>();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            options.AddPolicy("CorsPolicy", builder =>
            {
                if (environment.IsDevelopment())
                {
                    // Allow any origin in development for easier testing
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                }
                else
                {
                    // Optimize: Use specific origins in production for security
                    // string[] allowedOrigins = ["http://localhost:3000", "https://khongthichgaubong.online"];

                    builder.AllowAnyOrigin()
                        //.WithOrigins(allowedOrigins)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials(); // Allow credentials for authenticated requests
                }
            });
        });
    }
}