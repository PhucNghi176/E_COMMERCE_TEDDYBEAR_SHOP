﻿using CONTRACT.CONTRACT.DOMAIN.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace COMMAND.PERSISTENCE.Configurations;
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("User");
        builder.HasIndex(x => x.Email).IsUnique();
        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(256);
        builder.HasIndex(x => x.UserName).IsUnique();
    }
}