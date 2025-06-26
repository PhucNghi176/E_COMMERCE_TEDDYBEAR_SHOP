using CONTRACT.CONTRACT.DOMAIN.Abstractions.Entities;

namespace CONTRACT.CONTRACT.DOMAIN.Entities;
public class User : Entity<int>, IAuditableEntity
{
    public required string UserName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
    public DateTimeOffset? DeletedOnUtc { get; set; }
}