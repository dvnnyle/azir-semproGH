using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using azir_sempro.Data;
using azir_sempro.Models;

namespace azir_sempro.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext database;
    private readonly PasswordHasher<User> passwordHasher = new();

    private readonly UserManager<IdentityUser> userManager;

    public AccountController(
AppDbContext database,
UserManager<IdentityUser> userManager)
{
this.database = database;
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
        // TODO: check credentials once a user store / auth exists
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

        if (await database.Users.AnyAsync(user => user.Email == model.Email))
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

var result = await userManager.CreateAsync(
    identityUser,
    model.Password);

if (!result.Succeeded)
{
    foreach (var error in result.Errors)
    {
        ModelState.AddModelError("", error.Description);
    }

    return View(model);
}

return RedirectToAction("Login");
    }

    [HttpGet("/profile")]
    public IActionResult Profile()
    {
        return View();
    }
}
