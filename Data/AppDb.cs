using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using azir_sempro.Models;

namespace azir_sempro.Data;

public class AppDbContext : IdentityDbContext<IdentityUser<int>, IdentityRole<int>, int>

{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Submission> Submissions { get; set; }
}
