namespace CONTRACT.CONTRACT.DOMAIN.Abstractions.Entities;

public interface IAuditableEntity
{
    DateTimeOffset CreatedOnUtc { get; set; }

    DateTimeOffset? ModifiedOnUtc { get; set; }

    DateTimeOffset? DeletedOnUtc { get; set; }
}