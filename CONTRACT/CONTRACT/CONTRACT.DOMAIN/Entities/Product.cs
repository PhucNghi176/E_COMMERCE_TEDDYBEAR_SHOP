using System.ComponentModel.DataAnnotations;
using CONTRACT.CONTRACT.DOMAIN.Abstractions.Entities;

namespace CONTRACT.CONTRACT.DOMAIN.Entities;
public class Product : Entity<int>, IAuditableEntity
{
    [MaxLength(200)] public string? Name { get; set; } = "New Product";
    [MaxLength(20)] public string? Size { get; set; } = "M"; // Default size set to "M"

    [Range(0, int.MaxValue, ErrorMessage = "Quantity must be a non-negative integer.")]
    public int Quantity { get; set; } = 0; // Default quantity set to 0

    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be a positive number.")]
    public decimal Price { get; set; } = 0.01m; // Default price set to 0.01

    [MaxLength(50)] public string[]? Color { get; set; }

    [MaxLength(500)]
    public string? PrimaryImgUrl { get; set; } = string.Empty; // Default primary image URL set to empty string

    public string[]? ImgUrl { get; set; }

    // Navigation property for tags
    public virtual ICollection<ProductTag> ProductTags { get; init; } = new List<ProductTag>();
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
    public DateTimeOffset? DeletedOnUtc { get; set; }
}