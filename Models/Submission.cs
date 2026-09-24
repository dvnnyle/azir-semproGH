namespace azir_sempro.Models;

// Speiler Data/sql/submissions.sql - EF Core matcher kolonner mot disse
// egenskapene etter navn, ingen migrasjoner brukes i dette prosjektet.
public class Submission
{
    public int SubmissionId { get; set; }
    // Nullable inntil innlogging finnes - kobles til en ekte bruker senere
    public int? UserId { get; set; }
    public string Tittel { get; set; } = "";
    public string Kategori { get; set; } = "";
    public string Farge { get; set; } = "gronn";
    public string Lokasjon { get; set; } = "";
    public string PunkterJson { get; set; } = "";
    public string? Beskrivelse { get; set; }
    public DateTime Tidspunkt { get; set; }
    public string Status { get; set; } = "ny";
}
