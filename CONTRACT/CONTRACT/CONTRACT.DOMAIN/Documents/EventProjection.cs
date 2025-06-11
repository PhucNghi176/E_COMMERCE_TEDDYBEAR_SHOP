using CONTRACT.CONTRACT.CONTRACT.Attributes;
using CONTRACT.CONTRACT.DOMAIN.Abstractions.Entities;
using CONTRACT.CONTRACT.DOMAIN.Constrants;

namespace CONTRACT.CONTRACT.DOMAIN.Documents;
[BsonCollection(TableNames.Event)]
public class EventProjection : Document
{
    public Guid EventId { get; set; }
    public required string Name { get; set; }
    public required string Type { get; set; }
}