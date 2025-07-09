using COMMAND.PERSISTENCE.Interceptors;
using COMMAND.PERSISTENCE.Repositories;
using CONTRACT.CONTRACT.DOMAIN.Abstractions.Repositories;
using CONTRACT.CONTRACT.PERSISTENCE.DependencyInjection.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace COMMAND.PERSISTENCE.DependencyInjection.Extensions;
public static class ServiceCollectionExtensions
{
    public static void AddSqlServerPersistence(this IServiceCollection services)
    {
        _ = services.AddDbContextPool<DbContext, ApplicationDbContext>((provider, builder) =>
        {
            // Interceptor
            var auditableInterceptor = provider.GetService<UpdateAuditableEntitiesInterceptor>();
            var deletableInterceptor = provider.GetService<DeleteAuditableEntitiesInterceptor>();
            // var convertCommandInterceptor = provider.GetService<CovertCommandToOutboxMessagesInterceptor>();

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
                            .CommandTimeout(30))
                .AddInterceptors(
                    auditableInterceptor,
                    deletableInterceptor);
            // ,convertCommandInterceptor

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

    public static void AddInterceptorPersistence(this IServiceCollection services)
    {
        _ = services.AddSingleton<UpdateAuditableEntitiesInterceptor>();
        _ = services.AddSingleton<DeleteAuditableEntitiesInterceptor>();
    }

    public static void AddRepositoryPersistence(this IServiceCollection services)
    {
        // Optimize: Use Scoped for better performance in web applications
        _ = services.AddScoped(typeof(IRepositoryBase<,>), typeof(RepositoryBase<,>));
        // services.AddTransient(typeof(IUnitOfWork), typeof(EFUnitOfWork));
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