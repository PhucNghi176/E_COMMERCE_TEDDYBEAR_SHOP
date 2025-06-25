using CONTRACT.CONTRACT.DOMAIN.Abstractions.Entities;

namespace CONTRACT.CONTRACT.DOMAIN.Entities;
public class Tag : Entity<int>, IAuditableEntity
{
    public string Name { get; set; }

    // Navigation property for products
    public virtual ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
    public DateTimeOffset? DeletedOnUtc { get; set; }
}