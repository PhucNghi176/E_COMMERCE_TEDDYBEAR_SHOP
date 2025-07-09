using CONTRACT.CONTRACT.DOMAIN.Abstractions.Repositories;
using CONTRACT.CONTRACT.PERSISTENCE.DependencyInjection.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
            var environment = provider.GetRequiredService<IHostEnvironment>();
            var options = provider.GetRequiredService<IOptionsMonitor<SqlServerRetryOptions>>();

            #region ============== OPTIMIZED-SQL-SERVER-CONFIGURATION ==============

            _ = builder
                // Optimize: Only enable detailed errors and sensitive data logging in development
                .EnableDetailedErrors(environment.IsDevelopment())
                .EnableSensitiveDataLogging(environment.IsDevelopment())
                // Optimize: Disable lazy loading to prevent N+1 queries and improve performance
                // .UseLazyLoadingProxies() // Commented out for better performance
                .UseSqlServer(
                    configuration.GetConnectionString("ConnectionStrings"),
                    optionsBuilder
                        => optionsBuilder.ExecutionStrategy(dependencies => new SqlServerRetryingExecutionStrategy(
                                dependencies,
                                options.CurrentValue.MaxRetryCount,
                                options.CurrentValue.MaxRetryDelay,
                                options.CurrentValue.ErrorNumbersToAdd))
                            .MigrationsAssembly(typeof(ApplicationDbContext).Assembly.GetName().Name)
                            // Optimize: Enable command timeout for long-running queries
                            .CommandTimeout(30));

            #endregion ============== OPTIMIZED-SQL-SERVER-CONFIGURATION ==============

            #region ============== LEGACY-SQL-SERVER-STRATEGY ==============

            //builder
            //.EnableDetailedErrors(true)
            //.EnableSensitiveDataLogging(true)
            //.UseLazyLoadingProxies(true) // => If UseLazyLoadingProxies, all of the navigation fields should be VIRTUAL
            //.UseSqlServer(
            //    connectionString: configuration.GetConnectionString("ConnectionStrings"),
            //        sqlServerOptionsAction: optionsBuilder
            //            => optionsBuilder
            //            .MigrationsAssembly(typeof(ApplicationDbContext).Assembly.GetName().Name));

            #endregion ============== LEGACY-SQL-SERVER-STRATEGY ==============
        });
    }

    public static void ConfigureServicesInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        _ = services.Configure<MongoDbSettings>(configuration.GetSection(nameof(MongoDbSettings)));

        _ = services.AddSingleton<IMongoDbSettings>(serviceProvider =>
            serviceProvider.GetRequiredService<IOptions<MongoDbSettings>>().Value);
    }

    public static void AddRepositoryPersistence(this IServiceCollection services)
    {
        // Optimize: Use Scoped for better performance in web applications
        _ = services.AddScoped(typeof(IRepositoryBase<,>), typeof(RepositoryBase<,>));
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