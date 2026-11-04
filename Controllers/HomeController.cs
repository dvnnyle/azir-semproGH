using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using azir_sempro.Data;

namespace azir_sempro.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext database;

    public HomeController(AppDbContext database)
    {
        this.database = database;
    }

    public async Task<IActionResult> Index()
    {
        var nylige = await database.Submissions
            .AsNoTracking()
            .OrderByDescending(submission => submission.Tidspunkt)
            .Take(3)
            .ToListAsync();

        return View(nylige);
    }
}
