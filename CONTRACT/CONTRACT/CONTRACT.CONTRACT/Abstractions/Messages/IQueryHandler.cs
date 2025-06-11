using CONTRACT.CONTRACT.CONTRACT.Abstractions.Shared;
using MediatR;

namespace CONTRACT.CONTRACT.CONTRACT.Abstractions.Messages;
public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>;