using CONTRACT.CONTRACT.CONTRACT.Abstractions.Shared;
using MediatR;

namespace CONTRACT.CONTRACT.CONTRACT.Abstractions.Messages;
public interface IQuery<TResponse> : IRequest<Result<TResponse>>;