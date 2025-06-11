using CONTRACT.CONTRACT.CONTRACT.Abstractions.Shared;
using MassTransit;
using MediatR;

namespace CONTRACT.CONTRACT.CONTRACT.Abstractions.Messages;

[ExcludeFromTopology]
public interface ICommand : IRequest<Result>;
[ExcludeFromTopology]
public interface ICommand<TResponse> : IRequest<Result<TResponse>>;
