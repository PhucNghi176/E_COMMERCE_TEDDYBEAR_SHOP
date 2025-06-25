using CONTRACT.CONTRACT.DOMAIN.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace COMMAND.PERSISTENCE.Configurations;
internal sealed class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        _ = builder.HasKey(x => x.Id);

        _ = builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        // Create unique index on Name
        _ = builder.HasIndex(x => x.Name)
            .IsUnique();
    }
}