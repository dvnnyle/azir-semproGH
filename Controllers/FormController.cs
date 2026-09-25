using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using azir_sempro.Data;
using azir_sempro.Models;

namespace azir_sempro.Controllers;

public class FormController : Controller
{
    private readonly AppDbContext database;
    private const string UtkastNokkel = "RessursUtkast";

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

        LagreUtkast(model);
        return RedirectToAction(nameof(Beskrivelse));
    }

    // Steg 2: fyll ut tittel, kategori og beskrivelse
    public IActionResult Beskrivelse()
    {
        var utkast = HentUtkast();
        if (utkast is null || string.IsNullOrWhiteSpace(utkast.PunkterJson))
        {
            return RedirectToAction(nameof(Draw));
        }

        return View(utkast);
    }

    [HttpPost]
    public IActionResult Beskrivelse(FormViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Tittel))
        {
            ModelState.AddModelError("", "Du ma gi ressursen en tittel.");
            return View(model);
        }

        LagreUtkast(model);
        return RedirectToAction(nameof(Oppsummering));
    }

    // Steg 3: oppsummering av alt som er fylt ut, les-modus for skjemaet
    public IActionResult Oppsummering()
    {
        var utkast = HentUtkast();
        if (utkast is null || string.IsNullOrWhiteSpace(utkast.Tittel))
        {
            return RedirectToAction(nameof(Draw));
        }

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
            Kategori = model.Kategori,
            Farge = model.Farge,
            Lokasjon = model.Lokasjon,
            PunkterJson = model.PunkterJson,
            Beskrivelse = model.Beskrivelse,
            Tidspunkt = DateTime.Now,
            Status = "ny"
        };
        database.Submissions.Add(innsending);
        await database.SaveChangesAsync();

        model.Tidspunkt = innsending.Tidspunkt;
        model.Status = innsending.Status;

        // Punkt 2 og 3: egen side viser innsendt skjema- og kartdata
        LagreUtkast(model);
        return RedirectToAction(nameof(Kvittering));
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
