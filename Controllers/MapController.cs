using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using azir_sempro.Data;
using azir_sempro.Models;

namespace azir_sempro.Controllers;

public class MapController : Controller
{
    private readonly AppDbContext database;
    public MapController(AppDbContext database) => this.database = database;

    public async Task<IActionResult> Index()
    {
        var kategorier = await database.Categories.AsNoTracking().ToListAsync();

        var submissions = await database.Submissions.AsNoTracking()
            .OrderByDescending(s => s.Tidspunkt)
            .ToListAsync();

        var ider = submissions.Select(s => s.SubmissionId).ToList();
        var punkter = await database.Points.AsNoTracking()
            .Where(p => ider.Contains(p.SubmissionId))
            .OrderBy(p => p.Nr)
            .GroupBy(p => p.SubmissionId)
            .ToDictionaryAsync(
                g => g.Key,
                g => JsonSerializer.Serialize(g.Select(p => new { lat = p.Breddegrad, lng = p.Lengdegrad }))
            );

        return View(new ResourceListViewModel
        {
            Submissions = submissions,
            KategoriNavn = kategorier.ToDictionary(c => c.CategoryId, c => c.Navn),
            PunkterPerSubmission = punkter
        });
    }
}