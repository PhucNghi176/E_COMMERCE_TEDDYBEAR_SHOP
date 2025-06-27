using CloudinaryDotNet;
using CONTRACT.CONTRACT.APPLICATION.Abstractions;
using CONTRACT.CONTRACT.INFRASTRUCTURE.Authentication;
using CONTRACT.CONTRACT.INFRASTRUCTURE.DependencyInjection.Options;
using CONTRACT.CONTRACT.INFRASTRUCTURE.Mail;
using CONTRACT.CONTRACT.INFRASTRUCTURE.Media;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace AUTHORIZATION.INFRASTRUCTURE.DependencyInjection.Extensions;
public static class ServiceCollectionExtensions
{
    public static void AddServicesInfrastructure(this IServiceCollection services)
    {
       services
            .AddTransient<IJwtTokenService, JwtTokenService>()
           // .AddTransient<ICacheService, CacheService>()
            .AddSingleton<IMediaService, CloudinaryService>()
            .AddSingleton<IMailService, MailService>()
            .AddSingleton<Cloudinary>(provider =>
            {
                var options = provider.GetRequiredService<IOptionsMonitor<CloudinaryOptions>>();
                return new Cloudinary(new Account(
                    options.CurrentValue.CloudName,
                    options.CurrentValue.ApiKey,
                    options.CurrentValue.ApiSecret));
            });
    }

    // Configure Redis
    public static void AddRedisInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Redis");

        _ = services.AddStackExchangeRedisCache(redisOptions => { redisOptions.Configuration = connectionString; });

        // Register Redis ConnectionMultiplexer as singleton
        _ = services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(connectionString ?? "localhost"));
    }

    public static void ConfigureMailOptionsInfrastructure(this IServiceCollection services,
        IConfigurationSection section)
    {
        _ = services
            .AddOptions<MailOption>()
            .Bind(section)
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }

    // Configure MediatR
    public static void AddMediatRInfrastructure(this IServiceCollection services)
    {
        _ = services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(AssemblyReference.Assembly));
    }
}