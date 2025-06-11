using COMMAND.PERSISTENCE;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace COMMAND.APPLICATION.Behaviors;
public sealed class TransactionPipelineBehavior<TRequest, TResponse>(ApplicationDbContext context)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request,
        RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!TransactionPipelineBehavior<TRequest, TResponse>.IsCommand()) // In case TRequest is QueryRequest just ignore
            return await next();

        #region ============== SQL-SERVER-STRATEGY-1 ==============

        //// Use of an EF Core resiliency strategy when using multiple DbContexts within an explicit BeginTransaction():
        //// https://learn.microsoft.com/ef/core/miscellaneous/connection-resiliency
        var strategy = context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
            {
                var response = await next();
                await context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return response;
            }
        });

        #endregion ============== SQL-SERVER-STRATEGY-1 ==============

        #region ============== SQL-SERVER-STRATEGY-2 ==============

        //IMPORTANT: passing "TransactionScopeAsyncFlowOption.Enabled" to the TransactionScope constructor. This is necessary to be able to use it with async/await.
        //using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //{
        //    var response = await next();
        //    await _unitOfWork.SaveChangesAsync(cancellationToken);
        //    transaction.Complete();
        //    await _unitOfWork.DisposeAsync();
        //    return response;
        //}

        #endregion ============== SQL-SERVER-STRATEGY-2 ==============
    }


    private static bool IsCommand()
    {
        return typeof(TRequest).Name.EndsWith("Command");
    }
}