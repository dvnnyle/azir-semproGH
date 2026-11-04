using System.ComponentModel.DataAnnotations;

namespace azir_sempro.Models;

public class ResourceListViewModel
{
    [StringLength(100)]
    public string Search { get; set; } = "";
    [StringLength(100)]
    public string Category { get; set; } = "";
    [RegularExpression("^(|ressurs|behov)$")]
    public string Type { get; set; } = "";
    [RegularExpression("^(|ny|vurderes|tildelt|lost)$")]
    public string Status { get; set; } = "";
    [Range(typeof(DateTime), "1900-01-01", "2100-12-31")]
    public DateTime? FromDate { get; set; }
    [Range(typeof(DateTime), "1900-01-01", "2100-12-31")]
    public DateTime? ToDate { get; set; }
    public List<string> Categories { get; set; } = [];
    public List<Submission> Submissions { get; set; } = [];
    // CategoryId -> Navn, siden Kategori na er en FK og ikke en kolonne pa Submission
    public Dictionary<int, string> KategoriNavn { get; set; } = [];
    // SubmissionId -> JSON-array av punkter ({lat,lng}), siden punktene na ligger i
    // en egen Points-tabell og ikke lenger pa Submission selv.
    public Dictionary<int, string> PunkterPerSubmission { get; set; } = [];
    // IdentityUserId -> visningsnavn (e-post/brukernavn), for a vise hvem som publiserte.
    public Dictionary<int, string> BrukerNavn { get; set; } = [];
}
