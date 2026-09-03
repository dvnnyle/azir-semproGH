using Microsoft.AspNetCore.Mvc;

namespace azir_sempro.Controllers;

public class MapController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}