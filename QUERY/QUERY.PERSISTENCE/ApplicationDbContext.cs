using CONTRACT.CONTRACT.DOMAIN.Entities;
using Microsoft.EntityFrameworkCore;

namespace QUERY.PERSISTENCE;
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public virtual DbSet<Tag> Tags { get; set; }
}