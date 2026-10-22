namespace azir_sempro.Models;

// Speiler Data/sql/submissions.sql - EF Core matcher kolonner mot disse
// egenskapene etter navn, ingen migrasjoner brukes i dette prosjektet.
public class Submission
{
    public int SubmissionId { get; set; }
    // Nullable inntil innlogging finnes - kobles til en ekte bruker senere
    public int? UserId { get; set; }
    public int CategoryId { get; set; }
    public string Tittel { get; set; } = "";
    public string Type { get; set; } = "ressurs";
    public string Adresse { get; set; } = "";
    // "hoy", "middels" eller "lav" - bare meningsfullt for behov. Ressurs-farge
    // pa kartet er alltid gronn, utledes fra Type, trenger ikke lagres.
    public string? Prioritet { get; set; }
    public DateTime? Frist { get; set; }
    public string? Beskrivelse { get; set; }
    public DateTime Tidspunkt { get; set; }
    public string Status { get; set; } = "ny";
}
