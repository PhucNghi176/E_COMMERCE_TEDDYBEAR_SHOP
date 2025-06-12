
using CONTRACT.CONTRACT.DOMAIN.Abstractions.Repositories;
using CONTRACT.CONTRACT.PERSISTENCE.DependencyInjection.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using QUERY.PERSISTENCE.Repositories;


namespace QUERY.PERSISTENCE.DependencyInjection.Extensions;
public static class ServiceCollectionExtensions
{
    public static void AddSqlServerPersistence(this IServiceCollection services)
    {
        _ = services.AddDbContextPool<DbContext, ApplicationDbContext>((provider, builder) =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            var options = provider.GetRequiredService<IOptionsMonitor<SqlServerRetryOptions>>();

            #region ============== SQL-SERVER-STRATEGY-1 ==============

            _ = builder
                .EnableDetailedErrors()
                .EnableSensitiveDataLogging()
                .UseLazyLoadingProxies() // => If UseLazyLoadingProxies, all of the navigation fields should be VIRTUAL
                .UseSqlServer(
                    configuration.GetConnectionString("ConnectionStrings"),
                    optionsBuilder
                        => optionsBuilder.ExecutionStrategy(dependencies => new SqlServerRetryingExecutionStrategy(
                                dependencies,
                                options.CurrentValue.MaxRetryCount,
                                options.CurrentValue.MaxRetryDelay,
                                options.CurrentValue.ErrorNumbersToAdd))
                            .MigrationsAssembly(typeof(ApplicationDbContext).Assembly.GetName().Name));

            #endregion ============== SQL-SERVER-STRATEGY-1 ==============

            #region ============== SQL-SERVER-STRATEGY-2 ==============

            //builder
            //.EnableDetailedErrors(true)
            //.EnableSensitiveDataLogging(true)
            //.UseLazyLoadingProxies(true) // => If UseLazyLoadingProxies, all of the navigation fields should be VIRTUAL
            //.UseSqlServer(
            //    connectionString: configuration.GetConnectionString("ConnectionStrings"),
            //        sqlServerOptionsAction: optionsBuilder
            //            => optionsBuilder
            //            .MigrationsAssembly(typeof(ApplicationDbContext).Assembly.GetName().Name));

            #endregion ============== SQL-SERVER-STRATEGY-2 ==============
        });
    }

    public static void ConfigureServicesInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        _ = services.Configure<MongoDbSettings>(configuration.GetSection(nameof(MongoDbSettings)));

        _ = services.AddSingleton<IMongoDbSettings>(serviceProvider =>
            serviceProvider.GetRequiredService<IOptions<MongoDbSettings>>().Value);

        _ = services.AddScoped(typeof(IMongoRepository<>), typeof(MongoRepository<>));
    }

    public static void AddRepositoryPersistence(this IServiceCollection services)
    {
        _ = services.AddTransient(typeof(IRepositoryBase<,>), typeof(RepositoryBase<,>));
    }

    public static OptionsBuilder<SqlServerRetryOptions> ConfigureSqlServerRetryOptionsPersistence(
        this IServiceCollection services, IConfigurationSection section)
    {
        return services
            .AddOptions<SqlServerRetryOptions>()
            .Bind(section)
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }

   
}