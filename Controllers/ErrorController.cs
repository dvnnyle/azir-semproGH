using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using azir_sempro.Models;

namespace azir_sempro.Controllers;

// Egen controller kun for feilsiden - flyttet ut av HomeController da Home-siden ble fjernet
public class ErrorController : Controller
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
