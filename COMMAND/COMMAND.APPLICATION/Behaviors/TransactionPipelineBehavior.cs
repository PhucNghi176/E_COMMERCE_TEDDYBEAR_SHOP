using COMMAND.PERSISTENCE;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace COMMAND.APPLICATION.Behaviors;
public sealed class TransactionPipelineBehavior<TRequest, TResponse>(
    ApplicationDbContext context,
    ILogger<TransactionPipelineBehavior<TRequest, TResponse>> logger
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request,
        RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Optimize: Skip transaction for non-command operations
        if (!IsCommand())
            return await next();

        // Optimize: Use execution strategy with better error handling
        var strategy = context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            try
            {
                await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
                
                logger.LogDebug("Started transaction for {RequestType}", typeof(TRequest).Name);
                
                var response = await next();
                
                var changeCount = await context.SaveChangesAsync(cancellationToken);
                logger.LogDebug("Saved {ChangeCount} changes for {RequestType}", changeCount, typeof(TRequest).Name);
                
                await transaction.CommitAsync(cancellationToken);
                logger.LogDebug("Committed transaction for {RequestType}", typeof(TRequest).Name);
                
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Transaction failed for {RequestType}: {ErrorMessage}", 
                    typeof(TRequest).Name, ex.Message);
                throw;
            }
        });
    }

    // Optimize: More efficient command detection
    private static bool IsCommand()
    {
        var requestType = typeof(TRequest);
        return requestType.Name.EndsWith("Command", StringComparison.OrdinalIgnoreCase) ||
               requestType.GetInterfaces().Any(i => i.Name.Contains("Command"));
    }
}