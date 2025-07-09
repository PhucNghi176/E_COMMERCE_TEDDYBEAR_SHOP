using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CONTRACT.CONTRACT.APPLICATION.Behaviors;
public class TracingPipelineBehavior<TRequest, TResponse>(ILogger<TRequest> logger) :
    IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Optimize: Use more efficient timing mechanism
        var startTime = Stopwatch.GetTimestamp();
        var response = await next();
        var elapsedTime = Stopwatch.GetElapsedTime(startTime);

        var elapsedMilliseconds = elapsedTime.TotalMilliseconds;
        var requestName = typeof(TRequest).Name;
        
        // Optimize: Only log in Debug/Development for better production performance
        logger.LogDebug("Request Details: {RequestName} completed in {ElapsedMilliseconds}ms. Request: {@Request}", 
            requestName, elapsedMilliseconds, request);

        return response;
    }
}