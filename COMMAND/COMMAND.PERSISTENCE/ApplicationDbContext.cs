using Microsoft.EntityFrameworkCore;

namespace COMMAND.PERSISTENCE;
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{

}