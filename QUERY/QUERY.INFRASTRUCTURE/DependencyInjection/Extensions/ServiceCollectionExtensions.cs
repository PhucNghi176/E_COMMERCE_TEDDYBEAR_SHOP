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

    // Configure for masstransit with rabbitMQ
    public static IServiceCollection AddMasstransitRabbitMqInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        var masstransitConfiguration = new MasstransitConfiguration();
        configuration.GetSection(nameof(MasstransitConfiguration)).Bind(masstransitConfiguration);

        var messageBusOption = new MessageBusOptions();
        configuration.GetSection(nameof(MessageBusOptions)).Bind(messageBusOption);

        _ = services.AddMassTransit(cfg =>
        {
            // ===================== Setup for Consumer =====================
            cfg.AddConsumers(Assembly
                .GetExecutingAssembly()); // Add all of consumers to masstransit instead above command

            // ?? => Configure endpoint formatter. Not configure for producer Root Exchange
            cfg.SetKebabCaseEndpointNameFormatter(); // ??

            cfg.UsingRabbitMq((context, bus) =>
            {
                bus.Host(masstransitConfiguration.Host, masstransitConfiguration.Port, masstransitConfiguration.VHost,
                    h =>
                    {
                        h.Username(masstransitConfiguration.UserName);
                        h.Password(masstransitConfiguration.Password);
                    });

                bus.UseMessageRetry(retry
                    => retry.Incremental(
                        messageBusOption.RetryLimit,
                        messageBusOption.InitialInterval,
                        messageBusOption.IntervalIncrement));

                bus.UseNewtonsoftJsonSerializer();

                bus.ConfigureNewtonsoftJsonSerializer(settings =>
                {
                    settings.Converters.Add(new TypeNameHandlingConverter(TypeNameHandling.Objects));
                    settings.Converters.Add(new DateOnlyJsonConverter());
                    settings.Converters.Add(new ExpirationDateOnlyJsonConverter());
                    return settings;
                });

                bus.ConfigureNewtonsoftJsonDeserializer(settings =>
                {
                    settings.Converters.Add(new TypeNameHandlingConverter(TypeNameHandling.Objects));
                    settings.Converters.Add(new DateOnlyJsonConverter());
                    settings.Converters.Add(new ExpirationDateOnlyJsonConverter());
                    return settings;
                });

                _ = bus.ConnectReceiveObserver(new LoggingReceiveObserver());
                _ = bus.ConnectConsumeObserver(new LoggingConsumeObserver());
                _ = bus.ConnectPublishObserver(new LoggingPublishObserver());
                _ = bus.ConnectSendObserver(new LoggingSendObserver());

                // Rename for Root Exchange and setup for consumer also
                bus.MessageTopology.SetEntityNameFormatter(new KebabCaseEntityNameFormatter());

                // ===================== Setup for Consumer =====================

                // Important to create Exchange and Queue
                bus.ConfigureEndpoints(context);
            });
        });

        return services;
    }

    public static void AddMediatRInfrastructure(this IServiceCollection services)
    {
        _ = services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(AssemblyReference.Assembly));
        // services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Application.AssemblyReference.Assembly));
    }
}