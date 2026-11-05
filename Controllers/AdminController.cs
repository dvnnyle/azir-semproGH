using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using azir_sempro.Data;
using azir_sempro.Models;

namespace azir_sempro.Controllers;

[Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Heimevernet}")]
public class AdminController : Controller
{
    private readonly AppDbContext database;

    public AdminController(AppDbContext database)
    {
        this.database = database;
    }

    public async Task<IActionResult> Index([FromQuery] AdminResourceOverviewViewModel filters)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var allSubmissions = await database.Submissions
            .AsNoTracking()
            .OrderByDescending(submission => submission.Tidspunkt)
            .ToListAsync();
        var categories = await database.Categories
            .AsNoTracking()
            .ToDictionaryAsync(category => category.CategoryId, category => category.Navn);
        filters.Categories = categories.Values.OrderBy(name => name).ToList();
        var users = await database.Users
            .AsNoTracking()
            .Select(user => new { user.Id, user.FirstName, user.LastName })
            .ToDictionaryAsync(user => user.Id, user => $"{user.FirstName} {user.LastName}".Trim());

        var visibleSubmissions = allSubmissions.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            filters.Search = filters.Search.Trim();
            visibleSubmissions = visibleSubmissions.Where(submission =>
                submission.Tittel.Contains(filters.Search, StringComparison.OrdinalIgnoreCase) ||
                submission.Adresse.Contains(filters.Search, StringComparison.OrdinalIgnoreCase) ||
                (submission.Beskrivelse?.Contains(filters.Search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                OwnerName(submission, users).Contains(filters.Search, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(filters.Type))
        {
            visibleSubmissions = visibleSubmissions.Where(submission => submission.Type == filters.Type);
        }

        if (!string.IsNullOrWhiteSpace(filters.Category))
        {
            visibleSubmissions = visibleSubmissions.Where(submission =>
                categories.GetValueOrDefault(submission.CategoryId) == filters.Category);
        }

        if (!string.IsNullOrWhiteSpace(filters.Status))
        {
            visibleSubmissions = visibleSubmissions.Where(submission => submission.Status == filters.Status);
        }

        filters.TotalCount = allSubmissions.Count;
        filters.ResourceCount = allSubmissions.Count(submission => submission.Type == "ressurs");
        filters.NeedCount = allSubmissions.Count(submission => submission.Type == "behov");
        filters.NewCount = allSubmissions.Count(submission => submission.Status == "ny");
        filters.Submissions = visibleSubmissions.Select(submission => new AdminSubmissionRowViewModel
        {
            SubmissionId = submission.SubmissionId,
            Tittel = submission.Tittel,
            Type = submission.Type,
            Prioritet = submission.Prioritet,
            Status = submission.Status,
            Adresse = submission.Adresse,
            CategoryName = categories.GetValueOrDefault(submission.CategoryId, "Ukjent kategori"),
            OwnerName = OwnerName(submission, users),
            Tidspunkt = submission.Tidspunkt
        }).ToList();

        return View(filters);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var submission = await database.Submissions
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.SubmissionId == id);
        if (submission is null)
        {
            return NotFound();
        }

        var punkter = await database.Points
            .AsNoTracking()
            .Where(point => point.SubmissionId == id)
            .OrderBy(point => point.Nr)
            .Select(point => new { lat = point.Breddegrad, lng = point.Lengdegrad })
            .ToListAsync();

        await PopulateCategoriesAsync();
        return View(new AdminSubmissionEditViewModel
        {
            Tittel = submission.Tittel,
            Type = submission.Type,
            Status = submission.Status,
            CategoryId = submission.CategoryId,
            Adresse = submission.Adresse,
            Prioritet = submission.Prioritet ?? "",
            Frist = submission.Frist,
            Beskrivelse = submission.Beskrivelse,
            PunkterJson = JsonSerializer.Serialize(punkter)
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, AdminSubmissionEditViewModel model)
    {
        var submission = await database.Submissions
            .FirstOrDefaultAsync(item => item.SubmissionId == id);
        if (submission is null)
        {
            return NotFound();
        }

        if (!await database.Categories.AnyAsync(category => category.CategoryId == model.CategoryId))
        {
            ModelState.AddModelError(nameof(model.CategoryId), "Velg en gyldig kategori.");
        }

        if (model.Type == "ressurs")
        {
            model.Prioritet = "";
        }
        else if (model.Type == "behov" && string.IsNullOrWhiteSpace(model.Prioritet))
        {
            ModelState.AddModelError(nameof(model.Prioritet), "Velg prioritet for et behov.");
        }

        if (model.Type != submission.Type &&
            await database.Requests.AnyAsync(request =>
                request.BehovId == id || request.RessursId == id))
        {
            ModelState.AddModelError(nameof(model.Type), "Typen kan ikke endres når meldingen er knyttet til en forespørsel.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateCategoriesAsync();
            return View(model);
        }

        submission.Tittel = model.Tittel.Trim();
        submission.Type = model.Type;
        submission.Status = model.Status;
        submission.CategoryId = model.CategoryId;
        submission.Adresse = model.Adresse.Trim();
        submission.Prioritet = model.Type == "behov" ? model.Prioritet : null;
        submission.Frist = model.Frist;
        submission.Beskrivelse = string.IsNullOrWhiteSpace(model.Beskrivelse) ? null : model.Beskrivelse.Trim();

        await database.SaveChangesAsync();
        TempData["AdminMessage"] = "Meldingen er oppdatert.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var submission = await database.Submissions
            .FirstOrDefaultAsync(item => item.SubmissionId == id);
        if (submission is null)
        {
            return NotFound();
        }

        var relatedRequests = await database.Requests
            .Where(request => request.BehovId == id || request.RessursId == id)
            .ToListAsync();
        await using var transaction = await database.Database.BeginTransactionAsync();
        database.Requests.RemoveRange(relatedRequests);
        await database.SaveChangesAsync();
        database.Submissions.Remove(submission);
        await database.SaveChangesAsync();
        await transaction.CommitAsync();

        TempData["AdminMessage"] = "Meldingen er slettet.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateCategoriesAsync()
    {
        ViewBag.Categories = await database.Categories
            .AsNoTracking()
            .OrderBy(category => category.Navn)
            .ToListAsync();
    }

    private static string OwnerName(Submission submission, IReadOnlyDictionary<int, string> users)
    {
        if (submission.IdentityUserId.HasValue &&
            users.TryGetValue(submission.IdentityUserId.Value, out var ownerName))
        {
            return ownerName;
        }

        return submission.UserId.HasValue
            ? $"Eldre bruker #{submission.UserId.Value}"
            : "Ukjent bruker";
    }
}
