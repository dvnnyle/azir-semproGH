using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using azir_sempro.Models;

namespace azir_sempro.Data;

public class AppDbContext : IdentityDbContext<IdentityUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Submission> Submissions { get; set; }
    public DbSet<Point> Points { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Request> Requests { get; set; }
    public DbSet<ResourceDraft> ResourceDrafts { get; set; }
}
