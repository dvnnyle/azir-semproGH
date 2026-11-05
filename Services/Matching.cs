using Microsoft.EntityFrameworkCore;
using azir_sempro.Data;
using azir_sempro.Models;

namespace azir_sempro.Services;

public record Sted(double Lat, double Lng);

public static class Matching
{
    private const double JordRadiusKm = 6371;

    public static double AvstandKm(Sted a, Sted b)
    {
        var dLat = TilRadianer(b.Lat - a.Lat);
        var dLng = TilRadianer(b.Lng - a.Lng);
        var h = Math.Pow(Math.Sin(dLat / 2), 2)
            + Math.Cos(TilRadianer(a.Lat)) * Math.Cos(TilRadianer(b.Lat)) * Math.Pow(Math.Sin(dLng / 2), 2);
        return 2 * JordRadiusKm * Math.Asin(Math.Sqrt(h));
    }

    // Midtpunktet gjør at nåler og tegnede områder kan sammenlignes.
    public static Dictionary<int, Sted> Steder(IEnumerable<Point> punkter) =>
        punkter
            .GroupBy(punkt => punkt.SubmissionId)
            .ToDictionary(
                gruppe => gruppe.Key,
                gruppe => new Sted(
                    gruppe.Average(punkt => (double)punkt.Breddegrad),
                    gruppe.Average(punkt => (double)punkt.Lengdegrad)));

    public static double? Avstand(int fraId, int tilId, IReadOnlyDictionary<int, Sted> steder) =>
        steder.TryGetValue(fraId, out var fra) && steder.TryGetValue(tilId, out var til)
            ? AvstandKm(fra, til)
            : null;

    public static List<Submission> Ranger(
        Submission behov,
        IEnumerable<Submission> ressurser,
        IReadOnlyDictionary<int, Sted> steder) =>
        ressurser
            .OrderByDescending(ressurs => ressurs.CategoryId == behov.CategoryId)
            .ThenBy(ressurs => Avstand(behov.SubmissionId, ressurs.SubmissionId, steder) ?? double.MaxValue)
            .ThenBy(ressurs => ressurs.Tidspunkt)
            .ToList();

    public static IEnumerable<Submission> EtterPrioritet(IEnumerable<Submission> behov) =>
        behov.OrderBy(PrioritetRekkefolge).ThenBy(item => item.Tidspunkt);

    // Høyest prioritet får forslag først, slik at samme ressurs ikke foreslås til to behov.
    public static List<(Submission Behov, Submission Ressurs)> Forslag(
        IEnumerable<Submission> behov,
        IEnumerable<Submission> ressurser,
        IReadOnlyDictionary<int, Sted> steder)
    {
        var ledige = ressurser.ToList();
        var par = new List<(Submission, Submission)>();

        foreach (var etBehov in EtterPrioritet(behov))
        {
            var beste = Ranger(etBehov, ledige.Where(ressurs => ressurs.CategoryId == etBehov.CategoryId), steder)
                .FirstOrDefault();
            if (beste is null)
            {
                continue;
            }

            par.Add((etBehov, beste));
            ledige.Remove(beste);
        }

        return par;
    }

    // Meldinger uten andre koblinger går tilbake til "vurderes", så en ressurs ikke blir stående opptatt.
    public static async Task FjernKoblingerAsync(AppDbContext database, List<Request> koblinger)
    {
        if (koblinger.Count == 0)
        {
            return;
        }

        database.Requests.RemoveRange(koblinger);
        await database.SaveChangesAsync();

        var berorte = koblinger
            .SelectMany(kobling => new[] { kobling.BehovId, kobling.RessursId })
            .Distinct()
            .ToList();
        await database.Submissions
            .Where(submission =>
                berorte.Contains(submission.SubmissionId) &&
                submission.Status == "tildelt" &&
                !database.Requests.Any(request =>
                    request.BehovId == submission.SubmissionId || request.RessursId == submission.SubmissionId))
            .ExecuteUpdateAsync(set => set.SetProperty(submission => submission.Status, "vurderes"));
    }

    private static int PrioritetRekkefolge(Submission behov) => behov.Prioritet switch
    {
        "høy" => 0,
        "middels" => 1,
        _ => 2
    };

    private static double TilRadianer(double grader) => grader * Math.PI / 180;
}
