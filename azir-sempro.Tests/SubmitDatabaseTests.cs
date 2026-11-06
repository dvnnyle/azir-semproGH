using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using azir_sempro.Controllers;
using azir_sempro.Data;
using azir_sempro.Models;

namespace azir_sempro.Tests;

// Tester at innsendingen faktisk lagres riktig. Bruker en midlertidig SQLite-database i minnet
// (forsvinner når testen er ferdig), så MySQL i Docker trengs ikke.
public class SubmitDatabaseTests : IDisposable
{
    private const string Omrade = """[{"lat":58.1,"lng":8.0},{"lat":58.2,"lng":8.1},{"lat":58.1,"lng":8.2}]""";

    private readonly SqliteConnection forbindelse;
    private readonly AppDbContext database;

    public SubmitDatabaseTests()
    {
        forbindelse = new SqliteConnection("DataSource=:memory:");
        forbindelse.Open();

        var valg = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(forbindelse).Options;
        database = new AppDbContext(valg);
        database.Database.EnsureCreated();

        database.Categories.Add(new Category { CategoryId = 1, Navn = "Mat" });
        database.SaveChanges();
    }

    public void Dispose()
    {
        database.Dispose();
        forbindelse.Dispose();
    }

    private FormController LagKontroller() => new(database, null!)
    {
        ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    [new Claim(ClaimTypes.NameIdentifier, "1")], "test"))
            }
        }
    };

    [Fact]
    public async Task Submit_Ressurs_LagresSomNyMedEttPunktPerHjorne()
    {
        var modell = new FormViewModel
        {
            Tittel = "Vannlager",
            Type = "ressurs",
            Prioritet = "høy",       // skal ignoreres for ressurser
            Adresse = "Elvegata 12",
            CategoryId = 1,
            PunkterJson = Omrade
        };

        var resultat = await LagKontroller().Submit(modell);

        var omdirigering = Assert.IsType<RedirectToActionResult>(resultat);
        Assert.Equal("Kvittering", omdirigering.ActionName);

        var lagret = await database.Submissions.SingleAsync();
        Assert.Equal("ny", lagret.Status);
        Assert.Null(lagret.Prioritet);
        Assert.Equal(1, lagret.IdentityUserId);
        Assert.Equal(3, await database.Points.CountAsync(punkt => punkt.SubmissionId == lagret.SubmissionId));
    }

    [Fact]
    public async Task Submit_BehovUtenPrioritet_LagresIkke()
    {
        var modell = new FormViewModel
        {
            Tittel = "Trenger mat",
            Type = "behov",
            Prioritet = null,
            Adresse = "Elvegata 12",
            CategoryId = 1,
            PunkterJson = Omrade
        };

        var resultat = await LagKontroller().Submit(modell);

        var view = Assert.IsType<ViewResult>(resultat);
        Assert.Equal("Oppsummering", view.ViewName);
        Assert.Empty(database.Submissions);
    }
}
