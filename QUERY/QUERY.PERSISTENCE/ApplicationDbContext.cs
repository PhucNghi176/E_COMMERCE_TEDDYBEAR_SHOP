using Microsoft.EntityFrameworkCore;

namespace QUERY.PERSISTENCE;
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
}