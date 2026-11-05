using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using System.Text.Json;
using azir_sempro.Data;
using azir_sempro.Models;

namespace azir_sempro.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext database;
    private readonly UserManager<ApplicationUser> userManager;
    private readonly SignInManager<ApplicationUser> signInManager;

    public AccountController(
        AppDbContext database,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        this.database = database;
        this.userManager = userManager;
        this.signInManager = signInManager;
    }

    [HttpGet("/login")]
    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginViewModel
        {
            ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : null
        });
    }

    [HttpPost("/login")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            isPersistent: false,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            if (Url.IsLocalUrl(model.ReturnUrl))
            {
                return LocalRedirect(model.ReturnUrl!);
            }

            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError("", "Feil e-post/passord eller kontoen er midlertidig låst.");
        return View(model);
    }

    [HttpGet("/register")]
    public IActionResult Register()
    {
        return View(new AccountViewModel());
    }

    [HttpPost("/register")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("registration")]
    public async Task<IActionResult> Register(AccountViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (await userManager.FindByEmailAsync(model.Email) is not null)
        {
            ModelState.AddModelError("Email", "Denne e-posten er allerede registrert.");
            return View(model);
        }

        if (await userManager.Users.AnyAsync(user => user.PhoneNumber == model.PhoneNumber))
        {
            ModelState.AddModelError("PhoneNumber", "Dette telefonnummeret er allerede registrert.");
            return View(model);
        }

        var identityUser = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
            FirstName = model.FirstName,
            LastName = model.LastName
        };

        var result = await userManager.CreateAsync(identityUser, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        var roleResult = await userManager.AddToRoleAsync(identityUser, AppRoles.Privatperson);
        if (!roleResult.Succeeded)
        {
            var rollback = await userManager.DeleteAsync(identityUser);
            if (!rollback.Succeeded)
            {
                var errors = string.Join("; ", rollback.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Could not roll back registration after role assignment failed: {errors}");
            }

            foreach (var error in roleResult.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        return RedirectToAction(nameof(Login));
    }

    [HttpPost("/logout")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet("/access-denied")]
    public IActionResult AccessDenied() => View();

    [HttpGet("/profile")]
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("An authenticated user must have an Identity user ID."));
        var categories = await database.Categories
            .AsNoTracking()
            .ToDictionaryAsync(category => category.CategoryId, category => category.Navn);
        var submissions = await database.Submissions
            .AsNoTracking()
            .Where(submission => submission.IdentityUserId == userId)
            .OrderByDescending(submission => submission.Tidspunkt)
            .ToListAsync();
        var drafts = await database.ResourceDrafts
            .AsNoTracking()
            .Where(draft => draft.IdentityUserId == userId)
            .OrderByDescending(draft => draft.UpdatedAt)
            .ToListAsync();

        return View(new AccountProfileViewModel
        {
            Submissions = submissions.Select(submission => new ProfileSubmissionViewModel
            {
                SubmissionId = submission.SubmissionId,
                Tittel = submission.Tittel,
                Type = submission.Type,
                Status = submission.Status,
                Adresse = submission.Adresse,
                CategoryName = categories.GetValueOrDefault(submission.CategoryId, "Ukjent kategori"),
                Tidspunkt = submission.Tidspunkt
            }).ToList(),
            Drafts = drafts.Select(draft =>
            {
                var form = JsonSerializer.Deserialize<FormViewModel>(draft.DataJson)
                    ?? throw new InvalidOperationException($"Draft {draft.ResourceDraftId} contains invalid form data.");
                return new ProfileDraftViewModel
                {
                    DraftId = draft.ResourceDraftId,
                    Tittel = form.Tittel,
                    Type = form.Type,
                    UpdatedAt = draft.UpdatedAt
                };
            }).ToList()
        });
    }

    // Lar en bruker slette sin egen melding fra profilsiden.
    // GetOwnedSubmissionAsync sjekker at meldingen tilhører innlogget bruker,
    // så ingen kan slette andres meldinger ved å endre id-en i adressen.
    [HttpPost("/profile/delete/{id:int}")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSubmission(int id)
    {
        var submission = await GetOwnedSubmissionAsync(id);
        if (submission is null)
        {
            return NotFound();
        }

        // Forespørsler som peker på meldingen må slettes først, ellers stopper
        // fremmednøklene i databasen slettingen (samme rekkefølge som i AdminController).
        var relatedRequests = await database.Requests
            .Where(request => request.BehovId == id || request.RessursId == id)
            .ToListAsync();

        await using var transaction = await database.Database.BeginTransactionAsync();
        database.Requests.RemoveRange(relatedRequests);
        await database.SaveChangesAsync();
        database.Submissions.Remove(submission);
        await database.SaveChangesAsync();
        await transaction.CommitAsync();

        TempData["ProfileMessage"] = "Meldingen er slettet.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpGet("/profile/edit/{id:int}")]
    [Authorize]
    public async Task<IActionResult> EditSubmission(int id)
    {
        var submission = await GetOwnedSubmissionAsync(id);
        if (submission is null)
        {
            return NotFound();
        }

        await PopulateCategoriesAsync();
        return View(new ProfileSubmissionEditViewModel
        {
            Tittel = submission.Tittel,
            CategoryId = submission.CategoryId,
            Adresse = submission.Adresse,
            Prioritet = submission.Prioritet ?? "",
            Frist = submission.Frist,
            Beskrivelse = submission.Beskrivelse,
            Type = submission.Type
        });
    }

    [HttpPost("/profile/edit/{id:int}")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditSubmission(int id, ProfileSubmissionEditViewModel model)
    {
        var submission = await GetOwnedSubmissionAsync(id);
        if (submission is null)
        {
            return NotFound();
        }

        if (!await database.Categories.AnyAsync(category => category.CategoryId == model.CategoryId))
        {
            ModelState.AddModelError(nameof(model.CategoryId), "Velg en gyldig kategori.");
        }

        if (submission.Type == "behov" && string.IsNullOrWhiteSpace(model.Prioritet))
        {
            ModelState.AddModelError(nameof(model.Prioritet), "Velg prioritet for et behov.");
        }

        if (!ModelState.IsValid)
        {
            model.Type = submission.Type;
            await PopulateCategoriesAsync();
            return View(model);
        }

        submission.Tittel = model.Tittel.Trim();
        submission.CategoryId = model.CategoryId;
        submission.Adresse = model.Adresse.Trim();
        submission.Prioritet = submission.Type == "behov" ? model.Prioritet : null;
        submission.Frist = model.Frist;
        submission.Beskrivelse = string.IsNullOrWhiteSpace(model.Beskrivelse) ? null : model.Beskrivelse.Trim();
        await database.SaveChangesAsync();

        TempData["ProfileMessage"] = "Meldingen er oppdatert.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpGet("/profile/delete-account")]
    [Authorize]
    public IActionResult DeleteAccount() => View(new DeleteAccountViewModel());

    // Sletter kontoen til innlogget bruker etter at passordet er bekreftet.
    // Brukerens meldinger, utkast og forespørsler som peker på meldingene slettes også,
    // slik at det ikke blir liggende ressurser og behov uten eier.
    [HttpPost("/profile/delete-account")]
    [Authorize]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> DeleteAccount(DeleteAccountViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            await signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }

        if (!await userManager.CheckPasswordAsync(user, model.Password))
        {
            ModelState.AddModelError(nameof(model.Password), "Feil passord.");
            return View(model);
        }

        var submissionIds = await database.Submissions
            .Where(submission => submission.IdentityUserId == user.Id)
            .Select(submission => submission.SubmissionId)
            .ToListAsync();

        // Alt skjer i én transaksjon: feiler noe, blir ingenting slettet.
        await using var transaction = await database.Database.BeginTransactionAsync();

        var relatedRequests = await database.Requests
            .Where(request => submissionIds.Contains(request.BehovId) || submissionIds.Contains(request.RessursId))
            .ToListAsync();
        database.Requests.RemoveRange(relatedRequests);
        await database.SaveChangesAsync();

        database.Submissions.RemoveRange(
            await database.Submissions.Where(submission => submission.IdentityUserId == user.Id).ToListAsync());
        database.ResourceDrafts.RemoveRange(
            await database.ResourceDrafts.Where(draft => draft.IdentityUserId == user.Id).ToListAsync());
        await database.SaveChangesAsync();

        // Roller og innloggingsdata slettes av databasen (ON DELETE CASCADE i identity.sql).
        var result = await userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            await transaction.RollbackAsync();
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View(model);
        }

        await transaction.CommitAsync();
        await signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    private async Task<Submission?> GetOwnedSubmissionAsync(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await database.Submissions.FirstOrDefaultAsync(submission =>
            submission.SubmissionId == id && submission.IdentityUserId == userId);
    }

    private async Task PopulateCategoriesAsync()
    {
        ViewBag.Categories = await database.Categories
            .AsNoTracking()
            .OrderBy(category => category.Navn)
            .ToListAsync();
    }
}