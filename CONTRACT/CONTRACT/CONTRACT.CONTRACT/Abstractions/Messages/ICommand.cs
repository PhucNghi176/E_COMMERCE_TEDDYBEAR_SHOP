using CONTRACT.CONTRACT.CONTRACT.Abstractions.Shared;
using MediatR;

namespace CONTRACT.CONTRACT.CONTRACT.Abstractions.Messages;
public interface ICommand : IRequest<Result>;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>;