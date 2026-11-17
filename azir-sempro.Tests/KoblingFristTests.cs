using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using azir_sempro.Controllers;
using azir_sempro.Data;
using azir_sempro.Models;

namespace azir_sempro.Tests;

// Tester at et behov eller en ressurs med utløpt frist ikke kan kobles.
// Bruker SQLite i minnet, som SubmitDatabaseTests.
public class KoblingFristTests : IDisposable
{
    private readonly SqliteConnection forbindelse;
    private readonly AppDbContext database;

    public KoblingFristTests()
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

    private AdminController LagKontroller()
    {
        var http = new DefaultHttpContext();
        return new AdminController(database)
        {
            ControllerContext = new ControllerContext { HttpContext = http },
            TempData = new TempDataDictionary(http, new IngenTempData())
        };
    }

    private int LeggTil(string type, DateTime? frist)
    {
        var innsending = new Submission
        {
            CategoryId = 1,
            Tittel = type,
            Type = type,
            Adresse = "Elvegata 12",
            Prioritet = type == "behov" ? "høy" : null,
            Frist = frist,
            Tidspunkt = DateTime.Now,
            Status = "ny"
        };
        database.Submissions.Add(innsending);
        database.SaveChanges();
        return innsending.SubmissionId;
    }

    [Theory]
    [InlineData(true, false)]   // behovet har utløpt frist
    [InlineData(false, true)]   // ressursen har utløpt frist
    public async Task Koble_UtloptFrist_KoblerIkke(bool behovUtlopt, bool ressursUtlopt)
    {
        var igaar = DateTime.Today.AddDays(-1);
        var behovId = LeggTil("behov", behovUtlopt ? igaar : null);
        var ressursId = LeggTil("ressurs", ressursUtlopt ? igaar : null);

        await LagKontroller().Koble(behovId, ressursId);

        database.ChangeTracker.Clear();
        Assert.Empty(database.Requests);
        Assert.All(database.Submissions, submission => Assert.Equal("ny", submission.Status));
    }

    [Fact]
    public async Task Koble_FristIDag_Kobles()
    {
        var behovId = LeggTil("behov", DateTime.Today);
        var ressursId = LeggTil("ressurs", null);

        await LagKontroller().Koble(behovId, ressursId);

        database.ChangeTracker.Clear();
        var kobling = Assert.Single(database.Requests);
        Assert.Equal(behovId, kobling.BehovId);
        Assert.All(database.Submissions, submission => Assert.Equal("tildelt", submission.Status));
    }

    private sealed class IngenTempData : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }
}
