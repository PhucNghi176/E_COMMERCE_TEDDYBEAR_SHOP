using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CONTRACT.CONTRACT.APPLICATION.Behaviors;
public class PerformancePipelineBehavior<TRequest, TResponse>(ILogger<TRequest> logger) :
    IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var startTime = Stopwatch.GetTimestamp();
        var response = await next(cancellationToken);
        var elapsedTime = Stopwatch.GetElapsedTime(startTime);

        var elapsedMilliseconds = elapsedTime.TotalMilliseconds;

        if (elapsedMilliseconds <= 5000)
            return response;

        var requestName = typeof(TRequest).Name;
        logger.LogWarning("Long Running Request - {RequestName} took {ElapsedMilliseconds}ms. Request: {@Request}",
            requestName, elapsedMilliseconds, request);

        return response;
    }
}