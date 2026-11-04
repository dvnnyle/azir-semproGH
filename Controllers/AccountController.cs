using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using azir_sempro.Models;

namespace azir_sempro.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> userManager;

    public AccountController(UserManager<ApplicationUser> userManager)
    {
        this.userManager = userManager;
    }

    [HttpGet("/login")]
    public IActionResult Login()
    {
        return View(new AccountViewModel());
    }

    [HttpPost("/login")]
    public IActionResult Login(AccountViewModel model)
    {
        // Innlogging er foreløpig ikke implementert.
        return View(model);
    }

    [HttpGet("/register")]
    public IActionResult Register()
    {
        return View(new AccountViewModel());
    }

    [HttpPost("/register")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(AccountViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (await userManager.Users.AnyAsync(user => user.Email == model.Email))
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
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        return RedirectToAction(nameof(Login));
    }

    [HttpGet("/profile")]
    public IActionResult Profile()
    {
        return View();
    }
}