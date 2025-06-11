using CONTRACT.CONTRACT.CONTRACT.Abstractions.Shared;
using MediatR;

namespace CONTRACT.CONTRACT.CONTRACT.Abstractions.Messages;
public interface ICommandHandler<TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand;

public interface ICommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>;