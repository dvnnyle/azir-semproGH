using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using azir_sempro.Data;
using azir_sempro.Models;
using azir_sempro.Services;

namespace azir_sempro.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext database;
    private readonly UserManager<ApplicationUser> userManager;
    private readonly SignInManager<ApplicationUser> signInManager;
    private readonly IEmailSender<ApplicationUser> emailSender;

    public AccountController(
        AppDbContext database,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IEmailSender<ApplicationUser> emailSender)
    {
        this.database = database;
        this.userManager = userManager;
        this.signInManager = signInManager;
        this.emailSender = emailSender;
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

            var bruker = await userManager.FindByEmailAsync(model.Email);
            if (bruker is not null &&
                (await userManager.IsInRoleAsync(bruker, AppRoles.Admin) ||
                 await userManager.IsInRoleAsync(bruker, AppRoles.Heimevernet)))
            {
                return RedirectToAction("Index", "Admin");
            }

            return RedirectToAction("Index", "Home");
        }

        // Egen melding når kontoen er låst etter for mange feil forsøk (Lockout i Program.cs:
        // 5 forsøk, 15 minutter), så brukeren skjønner hvorfor riktig passord heller ikke virker.
        ModelState.AddModelError("", result.IsLockedOut
            ? "For mange mislykkede forsøk. Kontoen er låst i 15 minutter. Du kan også bruke «Glemt passord?»."
            : "Feil e-post eller passord.");
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

    [HttpGet("/forgot-password")]
    public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

    // Lager en engangslenke for å sette nytt passord. Svaret er alltid det samme,
    // uansett om e-posten finnes, slik at ingen kan bruke skjemaet til å finne ut
    // hvilke e-poster som er registrert.
    [HttpPost("/forgot-password")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await userManager.FindByEmailAsync(model.Email.Trim());
        if (user?.Email is not null)
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var link = Url.Action(nameof(ResetPassword), "Account",
                new { email = user.Email, code }, Request.Scheme)!;
            await emailSender.SendPasswordResetLinkAsync(user, user.Email, link);
        }

        ViewData["Sent"] = true;
        return View(new ForgotPasswordViewModel());
    }

    [HttpGet("/reset-password")]
    public IActionResult ResetPassword(string? email, string? code)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code))
        {
            return RedirectToAction(nameof(ForgotPassword));
        }

        return View(new ResetPasswordViewModel { Email = email, Code = code });
    }

    [HttpPost("/reset-password")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await userManager.FindByEmailAsync(model.Email);
        if (user is null)
        {
            // Avslør ikke at e-posten ikke finnes.
            ViewData["Done"] = true;
            return View(model);
        }

        string token;
        try
        {
            token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Code));
        }
        catch (FormatException)
        {
            ModelState.AddModelError("", "Lenken er ugyldig eller utløpt. Be om en ny.");
            return View(model);
        }

        var result = await userManager.ResetPasswordAsync(user, token, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Code == "InvalidToken"
                    ? "Lenken er ugyldig eller utløpt. Be om en ny."
                    : error.Description);
            }
            return View(model);
        }

        // En bruker som var låst ute etter for mange feil forsøk, kan logge inn igjen.
        await userManager.SetLockoutEndDateAsync(user, null);
        await userManager.ResetAccessFailedCountAsync(user);

        ViewData["Done"] = true;
        return View(model);
    }

    [HttpGet("/access-denied")]
    public IActionResult AccessDenied() => View();

    [HttpGet("/profile")]
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("An authenticated user must have an Identity user ID."));
        var currentUser = await userManager.GetUserAsync(User)
            ?? throw new InvalidOperationException("An authenticated user must exist.");
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

        // Viser hva meldingene er koblet til. Bare tittelen vises, ikke kontaktinfo til den andre parten.
        var mineIder = submissions.Select(submission => submission.SubmissionId).ToList();
        var koblinger = await database.Requests
            .AsNoTracking()
            .Where(request => mineIder.Contains(request.BehovId) || mineIder.Contains(request.RessursId))
            .ToListAsync();
        var koblingIder = koblinger.SelectMany(request => new[] { request.BehovId, request.RessursId }).Distinct().ToList();
        var titler = await database.Submissions
            .AsNoTracking()
            .Where(submission => koblingIder.Contains(submission.SubmissionId))
            .ToDictionaryAsync(submission => submission.SubmissionId, submission => submission.Tittel);

        return View(new AccountProfileViewModel
        {
            FirstName = currentUser.FirstName,
            LastName = currentUser.LastName,
            Email = currentUser.Email ?? "",
            PhoneNumber = currentUser.PhoneNumber ?? "",
            Submissions = submissions.Select(submission => new ProfileSubmissionViewModel
            {
                SubmissionId = submission.SubmissionId,
                Tittel = submission.Tittel,
                Type = submission.Type,
                Status = submission.Status,
                Adresse = submission.Adresse,
                CategoryName = categories.GetValueOrDefault(submission.CategoryId, "Ukjent kategori"),
                Tidspunkt = submission.Tidspunkt,
                KobletTil = string.Join(", ", koblinger
                    .Where(request => request.BehovId == submission.SubmissionId || request.RessursId == submission.SubmissionId)
                    .Select(request => request.BehovId == submission.SubmissionId ? request.RessursId : request.BehovId)
                    .Select(id => titler.GetValueOrDefault(id))
                    .OfType<string>())
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
        await Matching.FjernKoblingerAsync(database, relatedRequests);
        database.Submissions.Remove(submission);
        await database.SaveChangesAsync();
        await transaction.CommitAsync();

        TempData["ProfileMessage"] = "Meldingen er slettet.";
        return RedirectToAction(nameof(Profile));
    }

    // Lar en bruker slette sitt eget upubliserte utkast.
    [HttpPost("/profile/delete-draft/{id:int}")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDraft(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var draft = await database.ResourceDrafts.FirstOrDefaultAsync(item =>
            item.ResourceDraftId == id && item.IdentityUserId == userId);
        if (draft is null)
        {
            return NotFound();
        }

        database.ResourceDrafts.Remove(draft);
        await database.SaveChangesAsync();

        TempData["ProfileMessage"] = "Utkastet er slettet.";
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

    [HttpGet("/settings")]
    [Authorize]
    public async Task<IActionResult> Settings()
    {
        var bruker = await userManager.GetUserAsync(User);
        if (bruker is null)
        {
            return Challenge();
        }

        ViewBag.Roller = string.Join(", ", await userManager.GetRolesAsync(bruker));
        return View(bruker);
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
        await Matching.FjernKoblingerAsync(database, relatedRequests);

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