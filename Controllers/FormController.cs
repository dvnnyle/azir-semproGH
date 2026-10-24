using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using azir_sempro.Data;
using azir_sempro.Models;

namespace azir_sempro.Controllers;

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
            ModelState.AddModelError("", "Du ma tegne et omrade eller sette en nal for a ga videre.");
            return View(model);
        }
        if (string.IsNullOrWhiteSpace(model.Adresse))
        {
            ModelState.AddModelError("", "Du ma skrive inn en adresse.");
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
        var harFeil = false;

        if (string.IsNullOrWhiteSpace(model.Tittel))
        {
            ModelState.AddModelError("", "Du ma gi ressursen en tittel.");
            harFeil = true;
        }
        if (model.CategoryId <= 0)
        {
            ModelState.AddModelError("", "Du ma velge en kategori.");
            harFeil = true;
        }
        if (string.IsNullOrWhiteSpace(model.Adresse))
        {
            ModelState.AddModelError("", "Du ma skrive inn en adresse.");
            harFeil = true;
        }

        if (harFeil)
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
        // UserId er nullable helt til innlogging finnes - se Data/sql/submissions.sql.
        // TODO: sett til den innloggede brukerens UserId nar auth er pa plass.
        var innsending = new Submission
        {
            Tittel = model.Tittel,
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

        // Punktene fra Leaflet (wire-format i PunkterJson) lagres som egne
        // rader i Points, en per hjornepunkt, i tegne-rekkefolge.
        // PropertyNameCaseInsensitive er nodvendig - JS sender sma bokstaver
        // ("lat"/"lng"), System.Text.Json er case-sensitive som standard (ulikt
        // Newtonsoft), sa uten denne matcher ingenting og alt blir stille 0.
        var punkter = JsonSerializer.Deserialize<List<PunktDto>>(model.PunkterJson, CaseInsensitiv) ?? [];
        for (var i = 0; i < punkter.Count; i++)
        {
            database.Points.Add(new Point
            {
                SubmissionId = innsending.SubmissionId,
                Nr = (short)i,
                Breddegrad = punkter[i].Lat,
                Lengdegrad = punkter[i].Lng
            });
        }
        await database.SaveChangesAsync();

        model.Tidspunkt = innsending.Tidspunkt;
        model.Status = innsending.Status;

        // Punkt 2 og 3: egen side viser innsendt skjema- og kartdata
        LagreUtkast(model);
        return RedirectToAction(nameof(Kvittering));
    }

    private record PunktDto(decimal Lat, decimal Lng);

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
