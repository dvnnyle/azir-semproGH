using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using azir_sempro.Data;
using azir_sempro.Models;

namespace azir_sempro.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext database;
       private readonly SignInManager<IdentityUser<int>> signInManager;

private readonly UserManager<IdentityUser<int>> userManager;

public AccountController(
    AppDbContext database,
    SignInManager<IdentityUser<int>> signInManager,
    UserManager<IdentityUser<int>> userManager)
{
    this.database = database;
    this.signInManager = signInManager;
    this.userManager = userManager;
}



       [HttpGet("/login")]
    public IActionResult Login()
    {
        return View(new LoginViewModel());
    }

    [HttpPost("/login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            false,
            false);

        if (result.Succeeded)
        {
            return RedirectToAction("Profile");
        }

        ModelState.AddModelError("", "Feil e-post eller passord.");
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

        if (await userManager.FindByEmailAsync(model.Email) != null)
        {
            ModelState.AddModelError("Email", "Denne e-posten er allerede registrert.");
            return View(model);
        }

        if (await database.Users.AnyAsync(user => user.PhoneNumber == model.PhoneNumber))
        {
            ModelState.AddModelError("PhoneNumber", "Dette telefonnummeret er allerede registrert.");
            return View(model);
        }

       var identityUser = new IdentityUser<int>
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

    [Authorize]
[HttpGet("/profile")]
public IActionResult Profile()
{
    return View();
}

}

 
