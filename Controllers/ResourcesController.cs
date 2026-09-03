using Microsoft.AspNetCore.Mvc;

namespace azir_sempro.Controllers;

public class ResourcesController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}