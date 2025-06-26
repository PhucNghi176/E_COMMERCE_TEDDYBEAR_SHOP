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
            // .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationDefaultBehavior<,>))
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>))
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformancePipelineBehavior<,>))
           // .AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingPipelineBehaviorCachingBehavior<,>))
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(TracingPipelineBehavior<,>))
            .AddValidatorsFromAssembly(CONTRACT.AssemblyReference.Assembly, includeInternalTypes: true);
    }
}