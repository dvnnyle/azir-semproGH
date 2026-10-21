using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using azir_sempro.Data;
using azir_sempro.Models;

namespace azir_sempro.Controllers;

public class ResourcesController : Controller
{
    private readonly AppDbContext database;

    public ResourcesController(AppDbContext database)
    {
        this.database = database;
    }

    public async Task<IActionResult> Index([FromQuery] ResourceListViewModel filters)
    {
        var categories = await database.Submissions
            .AsNoTracking()
            .Where(submission => submission.Kategori != "")
            .Select(submission => submission.Kategori)
            .Distinct()
            .OrderBy(category => category)
            .ToListAsync();

        var submissions = database.Submissions.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            var search = filters.Search.Trim();
            submissions = submissions.Where(submission =>
                submission.Tittel.Contains(search) ||
                submission.Kategori.Contains(search) ||
                submission.Lokasjon.Contains(search) ||
                (submission.Beskrivelse != null && submission.Beskrivelse.Contains(search)));
            filters.Search = search;
        }

        if (!string.IsNullOrWhiteSpace(filters.Category))
        {
            submissions = submissions.Where(submission => submission.Kategori == filters.Category);
        }

        if (!string.IsNullOrWhiteSpace(filters.Type))
        {
            submissions = submissions.Where(submission => submission.Type == filters.Type);
        }

        if (!string.IsNullOrWhiteSpace(filters.Status))
        {
            submissions = submissions.Where(submission => submission.Status == filters.Status);
        }

        if (filters.FromDate.HasValue)
        {
            submissions = submissions.Where(submission => submission.Tidspunkt >= filters.FromDate.Value.Date);
        }

        if (filters.ToDate.HasValue)
        {
            var dayAfterToDate = filters.ToDate.Value.Date.AddDays(1);
            submissions = submissions.Where(submission => submission.Tidspunkt < dayAfterToDate);
        }

        filters.Categories = categories;
        filters.Submissions = await submissions
            .OrderByDescending(submission => submission.Tidspunkt)
            .ToListAsync();

        return View(filters);
    }
}
