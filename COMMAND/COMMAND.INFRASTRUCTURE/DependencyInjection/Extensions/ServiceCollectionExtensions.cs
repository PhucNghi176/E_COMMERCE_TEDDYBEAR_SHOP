using CloudinaryDotNet;
using COMMAND.INFRASTRUCTURE.BackgroundJobs;
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
using System.Reflection;

namespace COMMAND.INFRASTRUCTURE.DependencyInjection.Extensions;
public static class ServiceCollectionExtensions
{
    public static void AddServicesInfrastructure(this IServiceCollection services)
    {
        services
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

        services.AddStackExchangeRedisCache(redisOptions => { redisOptions.Configuration = connectionString; });

        // Register Redis ConnectionMultiplexer as singleton
        services.AddSingleton<IConnectionMultiplexer>(sp =>
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

        services.AddMassTransit(cfg =>
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

                bus.ConnectReceiveObserver(new LoggingReceiveObserver());
                bus.ConnectConsumeObserver(new LoggingConsumeObserver());
                bus.ConnectPublishObserver(new LoggingPublishObserver());
                bus.ConnectSendObserver(new LoggingSendObserver());

                // Rename for Root Exchange and setup for consumer also
                bus.MessageTopology.SetEntityNameFormatter(new KebabCaseEntityNameFormatter());

                // ===================== Setup for Consumer =====================

                // Important to create Exchange and Queue
                bus.ConfigureEndpoints(context);
            });
        });

        return services;
    }

    // Configure Job
    public static void AddQuartzInfrastructure(this IServiceCollection services)
    {
        services.AddQuartz(configure =>
        {
            // Process outbox messages job
            var outboxJobKey = new JobKey(nameof(ProcessOutboxMessagesJob));
            configure
                .AddJob<ProcessOutboxMessagesJob>(outboxJobKey)
                .AddTrigger(trigger =>
                    trigger.ForJob(outboxJobKey)
                        .WithSimpleSchedule(schedule =>
                            schedule.WithInterval(TimeSpan.FromMicroseconds(100))
                                .RepeatForever()));


            configure.UseMicrosoftDependencyInjectionJobFactory();
        });

        services.AddQuartzHostedService();
    }

    public static void ConfigureCloudinaryOptionsInfrastructure(this IServiceCollection services,
        IConfigurationSection section)
    {
        services
            .AddOptions<CloudinaryOptions>()
            .Bind(section)
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }

    public static void ConfigureMailOptionsInfrastructure(this IServiceCollection services,
        IConfigurationSection section)
    {
        services
            .AddOptions<MailOption>()
            .Bind(section)
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }

    // Configure MediatR
    public static void AddMediatRInfrastructure(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(AssemblyReference.Assembly));
    }
}