using System.ComponentModel.DataAnnotations;

namespace azir_sempro.Models;

public class EndreRessursViewModel
{
    public int SubmissionId { get; set; }

    [Required]
    public string Tittel { get; set; } = "";

    public string? Beskrivelse { get; set; }
}