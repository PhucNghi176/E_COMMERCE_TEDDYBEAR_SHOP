using CONTRACT.CONTRACT.DOMAIN.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace COMMAND.PERSISTENCE.Configurations;
internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        _ = builder.HasKey(x => x.Id);

        _ = builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        _ = builder.Property(x => x.Size)
            .HasMaxLength(50);

        _ = builder.Property(x => x.Color)
            .HasMaxLength(50);

        _ = builder.Property(x => x.Price)
            .HasColumnType("decimal(18,2)");

        _ = builder.Property(x => x.ImgUrl)
            .HasMaxLength(500);

        _ = builder.Property(x => x.CreatedOnUtc)
            .IsRequired();

        // Configure soft delete
        _ = builder.HasQueryFilter(x => !x.IsDeleted);
    }
}