using MassTransit;

namespace CONTRACT.CONTRACT.CONTRACT.Abstractions.Messages;

[ExcludeFromTopology]
public interface IDomainEvent
{
    public Guid IdEvent { get; init; }
}