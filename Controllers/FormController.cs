using Microsoft.AspNetCore.Mvc;
using azir_sempro.Models;

namespace azir_sempro.Controllers;

public class FormController : Controller
{
    // Punkt 2: in-memory liste, ingen database enda
    private static readonly List<FormViewModel> innsendinger = [];

    public IActionResult Index()
    {
        // Punkt 1: modell i stedet for ViewBag
        return View(new FormViewModel());
    }

    [HttpPost]
    public IActionResult Submit(FormViewModel model)
    {
        innsendinger.Add(model);

        // Punkt 2: egen side viser innsendt data
        return RedirectToAction("Innsendinger");
    }

    public IActionResult Innsendinger()
    {
        return View(innsendinger);
    }
}