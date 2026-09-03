using Microsoft.AspNetCore.Mvc;

namespace azir_sempro.Controllers;

public class DannyController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}