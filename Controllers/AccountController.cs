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

    public AccountController(AppDbContext database)
    {
        this.database = database;
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

        if (await database.Users.AnyAsync(user => user.Phone == model.PhoneNumber))
        {
            ModelState.AddModelError("PhoneNumber", "Dette telefonnummeret er allerede registrert.");
            return View(model);
        }

        var user = new User
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Phone = model.PhoneNumber,
            Email = model.Email
        };

        user.PasswordHash = passwordHasher.HashPassword(user, model.Password);
        database.Users.Add(user);
        await database.SaveChangesAsync();

        return RedirectToAction("Login");
    }

    [HttpGet("/profile")]
    public IActionResult Profile()
    {
        return View();
    }
}
