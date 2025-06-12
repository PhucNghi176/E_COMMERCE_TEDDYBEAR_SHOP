using Microsoft.Extensions.DependencyInjection;

namespace CONTRACT.CONTRACT.API.DependencyInjection.Extensions;
public static class ServiceCollectionExtensions
{
    public static void ConfigureCors(this IServiceCollection services)
    {
        _ = services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", builder =>
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
        });
    }
}