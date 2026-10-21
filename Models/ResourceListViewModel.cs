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
}
