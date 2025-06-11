using CONTRACT.CONTRACT.DOMAIN.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace COMMAND.PERSISTENCE.Configurations;
internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(x => x.Size)
            .HasMaxLength(50);
            
        builder.Property(x => x.Color)
            .HasMaxLength(50);
            
        builder.Property(x => x.Price)
            .HasColumnType("decimal(18,2)");
            
        builder.Property(x => x.ImgUrl)
            .HasMaxLength(500);
            
        builder.Property(x => x.CreatedOnUtc)
            .IsRequired();
            
        // Configure soft delete
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
