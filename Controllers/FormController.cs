using System.Text.Json;
using System.Text.Json.Serialization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using azir_sempro.Data;
using azir_sempro.Models;

namespace azir_sempro.Controllers;

[Authorize]
public class FormController : Controller
{
    private readonly AppDbContext database;
    private static readonly JsonSerializerOptions CaseInsensitiv = new() { PropertyNameCaseInsensitive = true };

    public FormController(AppDbContext database)
    {
        this.database = database;
    }

    public IActionResult Index() => RedirectToAction(nameof(Draw));

    // Steg 1: tegn omrade eller slipp en nal pa kartet
    public async Task<IActionResult> Draw(int? draftId)
    {
        var model = draftId.HasValue
            ? await HentUtkastAsync(draftId.Value)
            : new FormViewModel();
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Draw(FormViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.PunkterJson))
        {
            ModelState.AddModelError(nameof(model.PunkterJson), "Du ma tegne et omrade eller sette en nal for a ga videre.");
        }
        if (string.IsNullOrWhiteSpace(model.Adresse))
        {
            ModelState.AddModelError(nameof(model.Adresse), "Du ma skrive inn en adresse.");
        }

        if (!TryParsePoints(model.PunkterJson, out _, out var pointsError))
        {
            ModelState.AddModelError(nameof(model.PunkterJson), pointsError);
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (!await LagreUtkastAsync(model))
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Beskrivelse), new { draftId = model.DraftId });
    }

    // Steg 2: fyll ut tittel, kategori og beskrivelse
    public async Task<IActionResult> Beskrivelse(int draftId)
    {
        var utkast = await HentUtkastAsync(draftId);
        if (utkast is null || string.IsNullOrWhiteSpace(utkast.PunkterJson))
        {
            return NotFound();
        }

        ViewBag.Kategorier = await database.Categories.OrderBy(c => c.Navn).ToListAsync();
        return View(utkast);
    }

    [HttpPost]
    public async Task<IActionResult> Beskrivelse(FormViewModel model)
    {
        await ValidateSubmissionFieldsAsync(model);
        if (!TryParsePoints(model.PunkterJson, out _, out var pointsError))
        {
            ModelState.AddModelError(nameof(model.PunkterJson), pointsError);
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Kategorier = await database.Categories.OrderBy(c => c.Navn).ToListAsync();
            return View(model);
        }

        if (!await LagreUtkastAsync(model))
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Oppsummering), new { draftId = model.DraftId });
    }

    // Steg 3: oppsummering av alt som er fylt ut, les-modus for skjemaet
    public async Task<IActionResult> Oppsummering(int draftId)
    {
        var utkast = await HentUtkastAsync(draftId);
        if (utkast is null || string.IsNullOrWhiteSpace(utkast.Tittel))
        {
            return NotFound();
        }

        var kategori = await database.Categories.FindAsync(utkast.CategoryId);
        ViewBag.KategoriNavn = kategori?.Navn ?? "";
        return View(utkast);
    }

    [HttpPost]
    public async Task<IActionResult> Submit(FormViewModel model)
    {
        await ValidateSubmissionFieldsAsync(model);
        var pointsValid = TryParsePoints(model.PunkterJson, out var points, out var pointsError);
        if (!pointsValid)
        {
            ModelState.AddModelError(nameof(model.PunkterJson), pointsError);
        }

        if (!ModelState.IsValid)
        {
            var category = await database.Categories.FindAsync(model.CategoryId);
            ViewBag.KategoriNavn = category?.Navn ?? "";
            return View(nameof(Oppsummering), model);
        }

        if (model.DraftId.HasValue &&
            !await database.ResourceDrafts.AnyAsync(draft =>
                draft.ResourceDraftId == model.DraftId.Value &&
                draft.IdentityUserId == User.FindFirstValue(ClaimTypes.NameIdentifier)))
        {
            return NotFound();
        }

        var innsending = new Submission
        {
            Tittel = model.Tittel,
            IdentityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
            CategoryId = model.CategoryId,
            Type = model.Type,
            Adresse = model.Adresse,
            // Prioritet gir bare mening for behov - ressurs er alltid gronn pa
            // kartet, utledes fra Type, trenger ikke lagres.
            Prioritet = model.Type == "behov" ? model.Prioritet : null,
            Frist = model.Frist,
            Beskrivelse = model.Beskrivelse,
            Tidspunkt = DateTime.Now,
            Status = "ny"
        };
        database.Submissions.Add(innsending);
        await database.SaveChangesAsync();

        for (var i = 0; i < points.Count; i++)
        {
            database.Points.Add(new Point
            {
                SubmissionId = innsending.SubmissionId,
                Nr = (short)i,
                Breddegrad = points[i].Lat,
                Lengdegrad = points[i].Lng
            });
        }
        await database.SaveChangesAsync();

        if (model.DraftId.HasValue)
        {
            var draft = await database.ResourceDrafts.FindAsync(model.DraftId.Value);
            if (draft is not null)
            {
                database.ResourceDrafts.Remove(draft);
                await database.SaveChangesAsync();
            }
        }

        return RedirectToAction(nameof(Kvittering), new { id = innsending.SubmissionId });
    }

    private sealed class PunktDto
    {
        [JsonRequired]
        public decimal Lat { get; init; }

        [JsonRequired]
        public decimal Lng { get; init; }
    }

    private async Task ValidateSubmissionFieldsAsync(FormViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Tittel))
        {
            ModelState.AddModelError(nameof(model.Tittel), "Du ma gi ressursen en tittel.");
        }

        if (string.IsNullOrWhiteSpace(model.Adresse))
        {
            ModelState.AddModelError(nameof(model.Adresse), "Du ma skrive inn en adresse.");
        }

        if (model.CategoryId <= 0 ||
            !await database.Categories.AnyAsync(category => category.CategoryId == model.CategoryId))
        {
            ModelState.AddModelError(nameof(model.CategoryId), "Du ma velge en gyldig kategori.");
        }

        if (model.Type == "behov" && string.IsNullOrWhiteSpace(model.Prioritet))
        {
            ModelState.AddModelError(nameof(model.Prioritet), "Du ma velge en prioritet for behov.");
        }

    }

    private bool TryParsePoints(
        string? json,
        out List<PunktDto> points,
        out string error)
    {
        points = [];
        error = "Kartpunktene er ugyldige. Tegn omradet eller sett en nal pa nytt.";

        if (string.IsNullOrWhiteSpace(json) || json.Length > 65536)
        {
            return false;
        }

        try
        {
            points = JsonSerializer.Deserialize<List<PunktDto>>(json, CaseInsensitiv) ?? [];
        }
        catch (JsonException)
        {
            return false;
        }

        if (points.Count == 0 || points.Count > 1000 ||
            (points.Count != 1 && points.Count < 3) ||
            points.Any(point =>
                point is null ||
                point.Lat is < -90 or > 90 ||
                point.Lng is < -180 or > 180))
        {
            points = [];
            return false;
        }

        return true;
    }

    // Kvittering etter innsending
    public async Task<IActionResult> Kvittering(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var submission = await database.Submissions
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.SubmissionId == id && item.IdentityUserId == userId);
        if (submission is null)
        {
            return NotFound();
        }

        var points = await database.Points
            .AsNoTracking()
            .Where(point => point.SubmissionId == id)
            .OrderBy(point => point.Nr)
            .Select(point => new { lat = point.Breddegrad, lng = point.Lengdegrad })
            .ToListAsync();

        return View(new FormViewModel
        {
            Tittel = submission.Tittel,
            CategoryId = submission.CategoryId,
            Type = submission.Type,
            Adresse = submission.Adresse,
            Prioritet = submission.Prioritet,
            Frist = submission.Frist,
            Beskrivelse = submission.Beskrivelse ?? "",
            Tidspunkt = submission.Tidspunkt,
            Status = submission.Status,
            PunkterJson = JsonSerializer.Serialize(points)
        });
    }

    private async Task<FormViewModel?> HentUtkastAsync(int draftId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var draft = await database.ResourceDrafts
            .AsNoTracking()
            .FirstOrDefaultAsync(item =>
                item.ResourceDraftId == draftId && item.IdentityUserId == userId);
        if (draft is null)
            return null;

        var model = JsonSerializer.Deserialize<FormViewModel>(draft.DataJson)
            ?? throw new InvalidOperationException($"Draft {draftId} contains invalid form data.");
        model.DraftId = draft.ResourceDraftId;
        return model;
    }

    private async Task<bool> LagreUtkastAsync(FormViewModel model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("An authenticated user must have an Identity user ID.");
        await using var transaction = await database.Database.BeginTransactionAsync();
        ResourceDraft draft;
        var isNew = !model.DraftId.HasValue;
        if (isNew)
        {
            draft = new ResourceDraft
            {
                IdentityUserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            database.ResourceDrafts.Add(draft);
            await database.SaveChangesAsync();
            model.DraftId = draft.ResourceDraftId;
        }
        else
        {
            var existingDraft = await database.ResourceDrafts.FirstOrDefaultAsync(item =>
                item.ResourceDraftId == model.DraftId && item.IdentityUserId == userId);
            if (existingDraft is null)
            {
                return false;
            }

            draft = existingDraft;
            draft.UpdatedAt = DateTime.UtcNow;
        }

        draft.DataJson = JsonSerializer.Serialize(model);
        await database.SaveChangesAsync();
        await transaction.CommitAsync();
        return true;
    }
}
