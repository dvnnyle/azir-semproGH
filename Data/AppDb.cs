using Microsoft.EntityFrameworkCore;
using azir_sempro.Models;

namespace azir_sempro.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Submission> Submissions { get; set; }
    public DbSet<Point> Points { get; set; }
}
