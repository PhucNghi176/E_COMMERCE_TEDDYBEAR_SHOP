using CONTRACT.CONTRACT.APPLICATION.Abstractions;
using CONTRACT.CONTRACT.APPLICATION.DependencyInjection.Options;
using CONTRACT.CONTRACT.CONTRACT.JsonConverters;
using CONTRACT.CONTRACT.INFRASTRUCTURE.Caching;
using CONTRACT.CONTRACT.INFRASTRUCTURE.DependencyInjection.Extensions;
using CONTRACT.CONTRACT.INFRASTRUCTURE.PipeObservers;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Reflection;

namespace QUERY.INFRASTRUCTURE.DependencyInjection.Extensions;
public static class ServiceCollectionExtensions
{
    public static void AddServicesInfrastructure(this IServiceCollection services)
    {
        _ = services.AddTransient<ICacheService, CacheService>();
    }

    // Configure Redis
    public static void AddRedisInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        _ = services.AddStackExchangeRedisCache(redisOptions =>
        {
            var connectionString = configuration.GetConnectionString("Redis");
            redisOptions.Configuration = connectionString;
        });
    }
    
    public static void AddMediatRInfrastructure(this IServiceCollection services)
    {
        _ = services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(AssemblyReference.Assembly));
        // services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Application.AssemblyReference.Assembly));
    }
}