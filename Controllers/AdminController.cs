using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using azir_sempro.Data;
using azir_sempro.Models;
using azir_sempro.Services;

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
        filters.Kommuner = allSubmissions
            .Select(submission => submission.Kommune)
            .OfType<string>()
            .Distinct()
            .OrderBy(kommune => kommune)
            .ToList();
        var brukerListe = await database.Users
            .AsNoTracking()
            .Select(user => new { user.Id, user.FirstName, user.LastName, user.PhoneNumber })
            .ToListAsync();
        var users = brukerListe.ToDictionary(user => user.Id, user => $"{user.FirstName} {user.LastName}".Trim());
        var telefoner = brukerListe.ToDictionary(user => user.Id, user => user.PhoneNumber);

        var visibleSubmissions = allSubmissions.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            var sok = filters.Search.Trim();
            filters.Search = sok;
            visibleSubmissions = visibleSubmissions.Where(submission =>
                submission.Tittel.Contains(sok, StringComparison.OrdinalIgnoreCase) ||
                submission.Adresse.Contains(sok, StringComparison.OrdinalIgnoreCase) ||
                (submission.Beskrivelse?.Contains(sok, StringComparison.OrdinalIgnoreCase) ?? false) ||
                OwnerName(submission, users).Contains(sok, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(filters.Kommune))
        {
            visibleSubmissions = visibleSubmissions.Where(submission => submission.Kommune == filters.Kommune);
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
            Kommune = submission.Kommune,
            Beskrivelse = submission.Beskrivelse,
            Frist = submission.Frist,
            OwnerPhone = submission.IdentityUserId is int eierId ? telefoner.GetValueOrDefault(eierId) : null,
            CategoryName = categories.GetValueOrDefault(submission.CategoryId, "Ukjent kategori"),
            OwnerName = OwnerName(submission, users),
            Tidspunkt = submission.Tidspunkt
        }).ToList();

        return View(filters);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, string? returnUrl)
    {
        var submission = await database.Submissions
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.SubmissionId == id);
        if (submission is null)
        {
            return NotFound();
        }

        await PopulateCategoriesAsync();
        ViewBag.TilbakeUrl = TilbakeUrl(returnUrl);
        return View(new AdminSubmissionEditViewModel
        {
            Tittel = submission.Tittel,
            Type = submission.Type,
            Status = submission.Status,
            CategoryId = submission.CategoryId,
            Adresse = submission.Adresse,
            Kommune = submission.Kommune,
            Prioritet = submission.Prioritet ?? "",
            Frist = submission.Frist,
            Beskrivelse = submission.Beskrivelse,
            PunkterJson = await HentPunkterJsonAsync(id)
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, AdminSubmissionEditViewModel model, string? returnUrl)
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
            ViewBag.TilbakeUrl = TilbakeUrl(returnUrl);
            return View(model);
        }

        submission.Tittel = model.Tittel.Trim();
        submission.Type = model.Type;
        submission.Status = model.Status;
        submission.CategoryId = model.CategoryId;
        submission.Adresse = model.Adresse.Trim();
        submission.Kommune = string.IsNullOrWhiteSpace(model.Kommune) ? null : model.Kommune.Trim();
        submission.Prioritet = model.Type == "behov" ? model.Prioritet : null;
        submission.Frist = model.Frist;
        submission.Beskrivelse = string.IsNullOrWhiteSpace(model.Beskrivelse) ? null : model.Beskrivelse.Trim();

        await database.SaveChangesAsync();
        TempData["AdminMessage"] = "Meldingen er oppdatert.";
        return LocalRedirect(TilbakeUrl(returnUrl));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id, string? returnUrl)
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
        await Matching.FjernKoblingerAsync(database, relatedRequests);
        database.Submissions.Remove(submission);
        await database.SaveChangesAsync();
        await transaction.CommitAsync();

        TempData["AdminMessage"] = "Meldingen er slettet.";
        return LocalRedirect(TilbakeUrl(returnUrl));
    }

    [HttpGet("/Admin/Kobling")]
    public async Task<IActionResult> Match()
    {
        var (_, steder, forslag) = await HentMatchGrunnlagAsync();
        var tilMelding = await HentMeldingMapperAsync(steder);
        var koblinger = await HentKoblingerAsync(tilMelding);
        var kobledeBehov = koblinger.Select(kobling => kobling.Behov.SubmissionId).ToHashSet();
        var aapneBehov = await database.Submissions
            .AsNoTracking()
            .Where(submission => submission.Type == "behov" && submission.Status != "lost")
            .ToListAsync();

        return View(new AdminMatchViewModel
        {
            AntallTildelt = koblinger.Count(kobling => !kobling.ErLost),
            AntallLost = koblinger.Count(kobling => kobling.ErLost),
            Behov = Matching.EtterPrioritet(aapneBehov.Where(behov => !kobledeBehov.Contains(behov.SubmissionId)))
                .Select(behov => new MatchBehovViewModel
                {
                    Melding = tilMelding(behov),
                    ForslagTittel = forslag.GetValueOrDefault(behov.SubmissionId)?.Tittel
                })
                .ToList()
        });
    }

    [HttpGet("/Admin/Kobling/arkiv")]
    public async Task<IActionResult> Arkiv(string? status)
    {
        var (_, steder, _) = await HentMatchGrunnlagAsync();
        var koblinger = await HentKoblingerAsync(await HentMeldingMapperAsync(steder));
        var aktivtFilter = status is "tildelt" or "lost" ? status : "alle";

        return View(new AdminMatchArkivViewModel
        {
            Filter = aktivtFilter,
            AntallTildelt = koblinger.Count(kobling => !kobling.ErLost),
            AntallLost = koblinger.Count(kobling => kobling.ErLost),
            Koblinger = koblinger
                .Where(kobling => aktivtFilter == "alle" || kobling.ErLost == (aktivtFilter == "lost"))
                .ToList()
        });
    }

    [HttpGet("/Admin/Kobling/{id:int}")]
    public async Task<IActionResult> MatchBehov(int id)
    {
        var behov = await database.Submissions
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.SubmissionId == id && item.Type == "behov");
        if (behov is null)
        {
            return RedirectToAction(nameof(Match));
        }

        var (ressurser, steder, forslag) = await HentMatchGrunnlagAsync();
        var tilMelding = await HentMeldingMapperAsync(steder);
        var beste = forslag.GetValueOrDefault(id)?.SubmissionId;

        return View(new AdminMatchBehovViewModel
        {
            Behov = tilMelding(behov),
            BehovPunkterJson = await HentPunkterJsonAsync(id),
            Kandidater = behov.Status == "lost"
                ? []
                : Matching.Ranger(behov, ressurser, steder)
                    .OrderByDescending(ressurs => ressurs.SubmissionId == beste)
                    .Select(ressurs => new MatchKandidatViewModel
                    {
                        Ressurs = tilMelding(ressurs),
                        AvstandKm = Matching.Avstand(id, ressurs.SubmissionId, steder),
                        Treff = ressurs.SubmissionId == beste
                            ? "Beste treff"
                            : ressurs.CategoryId == behov.CategoryId ? "Samme kategori" : "Annen kategori"
                    })
                    .ToList()
        });
    }

    [HttpPost]
    public async Task<IActionResult> Koble(int id, int ressursId)
    {
        TempData["AdminMessage"] = await KobleAsync(id, ressursId)
            ? "Ressursen er koblet til behovet."
            : "Kunne ikke koble. Ressursen er ikke lenger ledig.";
        return RedirectToAction(nameof(Match));
    }

    [HttpPost]
    public async Task<IActionResult> FjernKobling(int id)
    {
        var kobling = await database.Requests.FirstOrDefaultAsync(request => request.RequestId == id);
        if (kobling is null)
        {
            return NotFound();
        }

        await using var transaction = await database.Database.BeginTransactionAsync();
        await Matching.FjernKoblingerAsync(database, [kobling]);
        await transaction.CommitAsync();

        TempData["AdminMessage"] = "Koblingen er fjernet.";
        return RedirectToAction(nameof(Arkiv));
    }

    [HttpPost]
    public async Task<IActionResult> MarkerLost(int id)
    {
        var kobling = await database.Requests
            .AsNoTracking()
            .FirstOrDefaultAsync(request => request.RequestId == id);
        if (kobling is null)
        {
            return NotFound();
        }

        var ressursIder = await database.Requests
            .Where(request => request.BehovId == kobling.BehovId)
            .Select(request => request.RessursId)
            .ToListAsync();
        await database.Submissions
            .Where(submission =>
                submission.SubmissionId == kobling.BehovId || ressursIder.Contains(submission.SubmissionId))
            .ExecuteUpdateAsync(set => set.SetProperty(submission => submission.Status, "lost"));

        TempData["AdminMessage"] = "Behovet og ressursen er markert som løst.";
        return RedirectToAction(nameof(Arkiv));
    }

    private string TilbakeUrl(string? returnUrl) =>
        Url.IsLocalUrl(returnUrl) ? returnUrl! : Url.Action(nameof(Index))!;

    private IQueryable<Submission> AapneBehov()
    {
        var idag = DateTime.Today;
        return database.Submissions
            .AsNoTracking()
            .Where(submission =>
                submission.Type == "behov" &&
                (submission.Status == "ny" || submission.Status == "vurderes") &&
                (submission.Frist == null || submission.Frist >= idag) &&
                !database.Requests.Any(request => request.BehovId == submission.SubmissionId));
    }

    private IQueryable<Submission> LedigeRessurser()
    {
        var idag = DateTime.Today;
        return database.Submissions
            .AsNoTracking()
            .Where(submission =>
                submission.Type == "ressurs" &&
                (submission.Status == "ny" || submission.Status == "vurderes") &&
                (submission.Frist == null || submission.Frist >= idag) &&
                !database.Requests.Any(request => request.RessursId == submission.SubmissionId));
    }

    private async Task<(List<Submission> Ressurser, Dictionary<int, Sted> Steder, Dictionary<int, Submission> Forslag)>
        HentMatchGrunnlagAsync()
    {
        var behov = await AapneBehov().ToListAsync();
        var ressurser = await LedigeRessurser().ToListAsync();
        var steder = Matching.Steder(await database.Points.AsNoTracking().ToListAsync());
        var forslag = Matching.Forslag(behov, ressurser, steder)
            .ToDictionary(par => par.Behov.SubmissionId, par => par.Ressurs);
        return (ressurser, steder, forslag);
    }

    private async Task<List<MatchKoblingViewModel>> HentKoblingerAsync(
        Func<Submission, MatchMeldingViewModel> tilMelding)
    {
        var koblinger = await database.Requests.AsNoTracking().ToListAsync();
        var meldinger = await database.Submissions
            .AsNoTracking()
            .Where(submission => database.Requests.Any(request =>
                request.BehovId == submission.SubmissionId || request.RessursId == submission.SubmissionId))
            .ToDictionaryAsync(submission => submission.SubmissionId);

        return koblinger
            .Select(kobling => new MatchKoblingViewModel
            {
                RequestId = kobling.RequestId,
                Behov = tilMelding(meldinger[kobling.BehovId]),
                Ressurs = tilMelding(meldinger[kobling.RessursId]),
                ErLost = meldinger[kobling.BehovId].Status == "lost",
                Tidspunkt = kobling.Tidspunkt
            })
            .OrderBy(kobling => kobling.ErLost)
            .ThenByDescending(kobling => kobling.Tidspunkt)
            .ToList();
    }

    private async Task<string> HentPunkterJsonAsync(int submissionId)
    {
        var punkter = await database.Points
            .AsNoTracking()
            .Where(point => point.SubmissionId == submissionId)
            .OrderBy(point => point.Nr)
            .Select(point => new { lat = point.Breddegrad, lng = point.Lengdegrad })
            .ToListAsync();
        return JsonSerializer.Serialize(punkter);
    }

    private async Task<Func<Submission, MatchMeldingViewModel>> HentMeldingMapperAsync(
        IReadOnlyDictionary<int, Sted> steder)
    {
        var categories = await database.Categories
            .AsNoTracking()
            .ToDictionaryAsync(category => category.CategoryId, category => category.Navn);
        var brukere = await database.Users
            .AsNoTracking()
            .Select(user => new { user.Id, user.FirstName, user.LastName, user.Email, user.PhoneNumber })
            .ToDictionaryAsync(user => user.Id);
        var brukernavn = brukere.ToDictionary(
            bruker => bruker.Key,
            bruker => $"{bruker.Value.FirstName} {bruker.Value.LastName}".Trim());

        return submission =>
        {
            var eier = submission.IdentityUserId is int eierId ? brukere.GetValueOrDefault(eierId) : null;
            var sted = steder.GetValueOrDefault(submission.SubmissionId);
            return new MatchMeldingViewModel
            {
                Lat = sted?.Lat,
                Lng = sted?.Lng,
                SubmissionId = submission.SubmissionId,
                Tittel = submission.Tittel,
                Type = submission.Type,
                Status = submission.Status,
                KategoriNavn = categories.GetValueOrDefault(submission.CategoryId, "Ukjent kategori"),
                Adresse = submission.Adresse,
                Prioritet = submission.Prioritet,
                Frist = submission.Frist,
                Beskrivelse = submission.Beskrivelse,
                Tidspunkt = submission.Tidspunkt,
                EierNavn = OwnerName(submission, brukernavn),
                EierEpost = eier?.Email,
                EierTelefon = eier?.PhoneNumber
            };
        };
    }

    private async Task<bool> KobleAsync(int behovId, int ressursId)
    {
        await using var transaction = await database.Database.BeginTransactionAsync();
        if (await database.Requests.AnyAsync(request => request.RessursId == ressursId))
        {
            return false;
        }

        // Betinget oppdatering: to samtidige koblinger kan ikke ta samme ressurs.
        var ressursTatt = await database.Submissions
            .Where(submission =>
                submission.SubmissionId == ressursId &&
                submission.Type == "ressurs" &&
                (submission.Status == "ny" || submission.Status == "vurderes"))
            .ExecuteUpdateAsync(set => set.SetProperty(submission => submission.Status, "tildelt"));
        var behovTildelt = await database.Submissions
            .Where(submission =>
                submission.SubmissionId == behovId &&
                submission.Type == "behov" &&
                submission.Status != "lost")
            .ExecuteUpdateAsync(set => set.SetProperty(submission => submission.Status, "tildelt"));
        if (ressursTatt == 0 || behovTildelt == 0)
        {
            return false;
        }

        database.Requests.Add(new Request
        {
            BehovId = behovId,
            RessursId = ressursId,
            Status = "godtatt",
            Tidspunkt = DateTime.Now
        });
        await database.SaveChangesAsync();
        await transaction.CommitAsync();
        return true;
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
