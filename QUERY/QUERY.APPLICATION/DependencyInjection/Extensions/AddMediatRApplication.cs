using CONTRACT.CONTRACT.APPLICATION.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace QUERY.APPLICATION.DependencyInjection.Extensions;
public static class ServiceCollectionExtensions
{
    public static void AddMediatRApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(AssemblyReference.Assembly)
            )
            // Optimize: Order pipeline behaviors for maximum performance in Query service
            // 1. Validation should come first to fail fast
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>))
            // 2. Caching is critical for Query service - place early to avoid expensive operations
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingPipelineBehaviorCachingBehavior<,>))
            // 3. Performance monitoring
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformancePipelineBehavior<,>))
            // 4. Tracing for debugging (queries don't need transactions)
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(TracingPipelineBehavior<,>))
            .AddValidatorsFromAssembly(CONTRACT.AssemblyReference.Assembly, includeInternalTypes: true);
    }
}