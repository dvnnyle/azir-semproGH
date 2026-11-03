using System.Text.Json;
using System.Text.Json.Serialization;
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
    private const string UtkastNokkel = "RessursUtkast";
    private static readonly JsonSerializerOptions CaseInsensitiv = new() { PropertyNameCaseInsensitive = true };

    public FormController(AppDbContext database)
    {
        this.database = database;
    }

    public IActionResult Index() => RedirectToAction(nameof(Draw));

    // Steg 1: tegn omrade eller slipp en nal pa kartet
    public IActionResult Draw()
    {
        return View(HentUtkast() ?? new FormViewModel());
    }

    [HttpPost]
    public IActionResult Draw(FormViewModel model)
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

        LagreUtkast(model);
        return RedirectToAction(nameof(Beskrivelse));
    }

    // Steg 2: fyll ut tittel, kategori og beskrivelse
    public async Task<IActionResult> Beskrivelse()
    {
        var utkast = HentUtkast();
        if (utkast is null || string.IsNullOrWhiteSpace(utkast.PunkterJson))
        {
            return RedirectToAction(nameof(Draw));
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

        LagreUtkast(model);
        return RedirectToAction(nameof(Oppsummering));
    }

    // Steg 3: oppsummering av alt som er fylt ut, les-modus for skjemaet
    public async Task<IActionResult> Oppsummering()
    {
        var utkast = HentUtkast();
        if (utkast is null || string.IsNullOrWhiteSpace(utkast.Tittel))
        {
            return RedirectToAction(nameof(Draw));
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

        // Submission.UserId points to the legacy Users table (integer IDs), not
        // ASP.NET Identity's AspNetUsers (string IDs), so it remains null for now.
        var innsending = new Submission
        {
            Tittel = model.Tittel,
            IdentityUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
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

        model.Tidspunkt = innsending.Tidspunkt;
        model.Status = innsending.Status;

        // Punkt 2 og 3: egen side viser innsendt skjema- og kartdata
        LagreUtkast(model);
        return RedirectToAction(nameof(Kvittering));
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
    public IActionResult Kvittering()
    {
        var utkast = HentUtkast();
        if (utkast is null)
        {
            return RedirectToAction(nameof(Draw));
        }

        return View(utkast);
    }

    // Utkastet lagres i TempData og "peekes" sa det overlever tilbake-navigering
    // mellom stegene (feks Rediger fra Oppsummering), uten a matte sende alle
    // feltene som skjulte felt gjennom hvert steg.
    private FormViewModel? HentUtkast()
    {
        if (TempData.Peek(UtkastNokkel) is not string json || string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        TempData.Keep(UtkastNokkel);
        return JsonSerializer.Deserialize<FormViewModel>(json);
    }

    private void LagreUtkast(FormViewModel model)
    {
        TempData[UtkastNokkel] = JsonSerializer.Serialize(model);
    }
}
