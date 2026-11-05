using azir_sempro.Models;
using azir_sempro.Services;

namespace azir_sempro.Tests;

public class MatchingTests
{
    private static Submission Melding(int id, string type, int kategori, string? prioritet = null) => new()
    {
        SubmissionId = id,
        Type = type,
        CategoryId = kategori,
        Prioritet = prioritet
    };

    [Fact]
    public void AvstandKm_EnBreddegradErOmtrent111Km()
    {
        var avstand = Matching.AvstandKm(new Sted(58, 8), new Sted(59, 8));

        Assert.InRange(avstand, 110, 112);
    }

    [Fact]
    public void Ranger_SetterSammeKategoriForanNaermereRessursIAnnenKategori()
    {
        var behov = Melding(1, "behov", 4, "høy");
        var fjernSammeKategori = Melding(2, "ressurs", 4);
        var naerAnnenKategori = Melding(3, "ressurs", 7);
        var naerSammeKategori = Melding(4, "ressurs", 4);
        var steder = new Dictionary<int, Sted>
        {
            [1] = new(58.15, 8.00),
            [2] = new(58.40, 8.00),
            [3] = new(58.15, 8.01),
            [4] = new(58.20, 8.00)
        };

        var rangert = Matching.Ranger(behov, [fjernSammeKategori, naerAnnenKategori, naerSammeKategori], steder);

        Assert.Equal([4, 2, 3], rangert.Select(ressurs => ressurs.SubmissionId));
    }

    [Fact]
    public void Ranger_SetterRessursUtenStedSistIKategorien()
    {
        var behov = Melding(1, "behov", 4);
        var utenSted = Melding(2, "ressurs", 4);
        var medSted = Melding(3, "ressurs", 4);
        var steder = new Dictionary<int, Sted> { [1] = new(58.15, 8.00), [3] = new(59, 9) };

        var rangert = Matching.Ranger(behov, [utenSted, medSted], steder);

        Assert.Equal([3, 2], rangert.Select(ressurs => ressurs.SubmissionId));
    }

    [Fact]
    public void Forslag_HoyPrioritetFaarRessursenOgDenForeslaasBareEnGang()
    {
        var lav = Melding(1, "behov", 4, "lav");
        var hoy = Melding(2, "behov", 4, "høy");
        var ressurs = Melding(3, "ressurs", 4);

        var par = Matching.Forslag([lav, hoy], [ressurs], new Dictionary<int, Sted>());

        var eneste = Assert.Single(par);
        Assert.Equal(2, eneste.Behov.SubmissionId);
        Assert.Equal(3, eneste.Ressurs.SubmissionId);
    }

    [Fact]
    public void Forslag_ForeslaarIkkeRessursIAnnenKategori()
    {
        var par = Matching.Forslag(
            [Melding(1, "behov", 4, "høy")],
            [Melding(2, "ressurs", 7)],
            new Dictionary<int, Sted>());

        Assert.Empty(par);
    }
}
