using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using azir_sempro.Models;

namespace azir_sempro.Controllers;

public class SkjermaController : Controller
{
    private const string InnsendingNokkel = "SkjermaInnsending";

    public IActionResult Index()
    {
        return View(new SkjermaViewModel());
    }

    [HttpPost]
    public IActionResult Index(SkjermaViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Tittel))
        {
            ModelState.AddModelError("", "Du må gi et navn.");
            return View(model);
        }

        TempData[InnsendingNokkel] = JsonSerializer.Serialize(model);
        return RedirectToAction(nameof(Kvittering));
    }

    // Egen side viser det innsendte skjemaet (punkt 2/3 i oppgaven)
    public IActionResult Kvittering()
    {
        if (TempData[InnsendingNokkel] is not string json || string.IsNullOrWhiteSpace(json))
        {
            return RedirectToAction(nameof(Index));
        }

        var model = JsonSerializer.Deserialize<SkjermaViewModel>(json);
        return View(model);
    }
}
