using CONTRACT.CONTRACT.DOMAIN.Abstractions.Entities;

namespace CONTRACT.CONTRACT.DOMAIN.Entities;
public class ProductTag : Entity<int>
{
    public int ProductId { get; set; }
    public int TagId { get; set; }

    // Navigation properties
    public virtual Product Product { get; set; }
    public virtual Tag Tag { get; set; }
}