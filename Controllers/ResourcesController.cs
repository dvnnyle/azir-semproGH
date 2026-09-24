using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using azir_sempro.Data;

namespace azir_sempro.Controllers;

public class ResourcesController : Controller
{
    private readonly AppDbContext database;

    public ResourcesController(AppDbContext database)
    {
        this.database = database;
    }

    public async Task<IActionResult> Index()
    {
        var alle = await database.Submissions.OrderByDescending(s => s.Tidspunkt).ToListAsync();
        return View(alle);
    }
}
