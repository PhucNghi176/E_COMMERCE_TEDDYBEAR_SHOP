using System.Reflection;
using CloudinaryDotNet;
using COMMAND.INFRASTRUCTURE;
using CONTRACT.CONTRACT.APPLICATION.Abstractions;
using CONTRACT.CONTRACT.CONTRACT.JsonConverters;
using CONTRACT.CONTRACT.INFRASTRUCTURE.Authentication;
using CONTRACT.CONTRACT.INFRASTRUCTURE.Caching;
using CONTRACT.CONTRACT.INFRASTRUCTURE.DependencyInjection.Extensions;
using CONTRACT.CONTRACT.INFRASTRUCTURE.DependencyInjection.Options;
using CONTRACT.CONTRACT.INFRASTRUCTURE.Mail;
using CONTRACT.CONTRACT.INFRASTRUCTURE.Media;
using CONTRACT.CONTRACT.INFRASTRUCTURE.PipeObservers;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Quartz;
using StackExchange.Redis;

namespace AUTHORIZATION.INFRASTRUCTURE.DependencyInjection.Extensions;
public static class ServiceCollectionExtensions
{
    public static void AddServicesInfrastructure(this IServiceCollection services)
    {
        _ = services
            .AddTransient<IJwtTokenService, JwtTokenService>()
            .AddTransient<ICacheService, CacheService>()
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

    // Configure for masstransit with rabbitMQ
    public static IServiceCollection AddMasstransitRabbitMQInfrastructure(this IServiceCollection services,
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