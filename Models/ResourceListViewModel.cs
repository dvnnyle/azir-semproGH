namespace azir_sempro.Models;

public class ResourceListViewModel
{
    public string Search { get; set; } = "";
    public string Category { get; set; } = "";
    public string Type { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public List<string> Categories { get; set; } = [];
    public List<Submission> Submissions { get; set; } = [];
    // CategoryId -> Navn, siden Kategori na er en FK og ikke en kolonne pa Submission
    public Dictionary<int, string> KategoriNavn { get; set; } = [];
    // SubmissionId -> JSON-array av punkter ({lat,lng}), siden punktene na ligger i
    // en egen Points-tabell og ikke lenger pa Submission selv.
    public Dictionary<int, string> PunkterPerSubmission { get; set; } = [];
}
