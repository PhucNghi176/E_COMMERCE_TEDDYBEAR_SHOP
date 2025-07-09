using COMMAND.APPLICATION.Behaviors;
using CONTRACT.CONTRACT.APPLICATION.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace COMMAND.APPLICATION.DependencyInjection.Extensions;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMediatRApplication(this IServiceCollection services)
    {
        return services
            .AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(AssemblyReference.Assembly)
            )
            // Optimize: Order pipeline behaviors for better performance
            // 1. Validation should come first to fail fast
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>))
            // 2. Caching before expensive operations
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingPipelineBehaviorCachingBehavior<,>))
            // 3. Performance monitoring
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformancePipelineBehavior<,>))
            // 4. Tracing for debugging
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(TracingPipelineBehavior<,>))
            // 5. Transaction should be last to wrap all operations
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionPipelineBehavior<,>))
            .AddValidatorsFromAssembly(CONTRACT.AssemblyReference.Assembly, includeInternalTypes: true);
    }

    //public static IServiceCollection PayOs(this IServiceCollection services)
    //{
    //    return services.AddTransient<IPaymentService, PaymentServices.PaymentServices>();
    //}
}