using System.Text.Json;
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
        var alleKategorier = await database.Categories
            .AsNoTracking()
            .OrderBy(category => category.Navn)
            .ToListAsync();
        var kategoriNavn = alleKategorier.ToDictionary(c => c.CategoryId, c => c.Navn);

        var submissions = database.Submissions.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            var search = filters.Search.Trim();
            submissions = submissions.Where(submission =>
                submission.Tittel.Contains(search) ||
                submission.Adresse.Contains(search) ||
                (submission.Beskrivelse != null && submission.Beskrivelse.Contains(search)));
            filters.Search = search;
        }

        if (!string.IsNullOrWhiteSpace(filters.Category))
        {
            var kategoriId = alleKategorier.FirstOrDefault(c => c.Navn == filters.Category)?.CategoryId;
            submissions = submissions.Where(submission => submission.CategoryId == kategoriId);
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

        var treffliste = await submissions
            .OrderByDescending(submission => submission.Tidspunkt)
            .ToListAsync();

        var submissionIder = treffliste.Select(s => s.SubmissionId).ToList();
        var punkterPerSubmission = await database.Points
            .AsNoTracking()
            .Where(p => submissionIder.Contains(p.SubmissionId))
            .OrderBy(p => p.Nr)
            .GroupBy(p => p.SubmissionId)
            .ToDictionaryAsync(
                gruppe => gruppe.Key,
                gruppe => JsonSerializer.Serialize(gruppe.Select(p => new { lat = p.Breddegrad, lng = p.Lengdegrad }))
            );

        filters.Categories = alleKategorier.Select(c => c.Navn).ToList();
        filters.KategoriNavn = kategoriNavn;
        filters.PunkterPerSubmission = punkterPerSubmission;
        filters.Submissions = treffliste;

        return View(filters);
    }
}
