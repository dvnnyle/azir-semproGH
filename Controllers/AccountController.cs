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
    private readonly UserManager<IdentityUser> userManager;
    private readonly SignInManager<IdentityUser> signInManager;

    public AccountController(
        AppDbContext database,
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager)
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

        var identityUser = new IdentityUser
        {
            UserName = model.Email,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber
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
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("An authenticated user must have an Identity user ID.");
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

    private async Task<Submission?> GetOwnedSubmissionAsync(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
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
