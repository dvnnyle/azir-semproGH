using Microsoft.AspNetCore.Mvc;

namespace azir_sempro.Controllers;

public class AccountController : Controller
{
    [HttpGet("/login")]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost("/login")]
    public IActionResult Login(string email, string password)
    {
        // TODO: check credentials once a user store / auth exists
        return View();
    }

    [HttpGet("/register")]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost("/register")]
    public IActionResult Register(string firstName, string lastName, string email, string phoneNumber, string password)
    {
        // TODO: create the user once a user store / auth exists
        return View();
    }

    [HttpGet("/profile")]
    public IActionResult Profile()
    {
        return View();
    }
}
