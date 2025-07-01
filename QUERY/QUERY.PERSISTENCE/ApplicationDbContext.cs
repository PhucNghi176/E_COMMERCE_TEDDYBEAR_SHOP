using CONTRACT.CONTRACT.DOMAIN.Entities;
using Microsoft.EntityFrameworkCore;

namespace QUERY.PERSISTENCE;
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public virtual DbSet<Tag> Tags { get; set; }
    public virtual DbSet<Product> Products { get; set; }
    public virtual DbSet<ProductTag> ProductTags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        modelBuilder.Entity<Product>().HasQueryFilter(x => !x.IsDeleted);
    }
}