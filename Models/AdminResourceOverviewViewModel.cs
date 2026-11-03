using System.ComponentModel.DataAnnotations;

namespace azir_sempro.Models;

public class AdminResourceOverviewViewModel
{
    [StringLength(100)]
    public string Search { get; set; } = "";

    [StringLength(100)]
    public string Category { get; set; } = "";

    [RegularExpression("^(|ressurs|behov)$")]
    public string Type { get; set; } = "";

    [RegularExpression("^(|ny|vurderes|tildelt|lost)$")]
    public string Status { get; set; } = "";

    public int TotalCount { get; set; }
    public int ResourceCount { get; set; }
    public int NeedCount { get; set; }
    public int NewCount { get; set; }
    public List<string> Categories { get; set; } = [];
    public List<AdminSubmissionRowViewModel> Submissions { get; set; } = [];
}

public class AdminSubmissionRowViewModel
{
    public int SubmissionId { get; set; }
    public string Tittel { get; set; } = "";
    public string Type { get; set; } = "";
    public string Status { get; set; } = "";
    public string Adresse { get; set; } = "";
    public string CategoryName { get; set; } = "";
    public string OwnerName { get; set; } = "";
    public DateTime Tidspunkt { get; set; }
}

public class AdminSubmissionEditViewModel
{
    [Required(ErrorMessage = "Skriv inn en tittel.")]
    [StringLength(255)]
    public string Tittel { get; set; } = "";

    [Required]
    [RegularExpression("^(ressurs|behov)$", ErrorMessage = "Velg ressurs eller behov.")]
    public string Type { get; set; } = "";

    [Required]
    [RegularExpression("^(ny|vurderes|tildelt|lost)$", ErrorMessage = "Velg en gyldig status.")]
    public string Status { get; set; } = "";

    [Range(1, int.MaxValue, ErrorMessage = "Velg en kategori.")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Skriv inn en adresse.")]
    [StringLength(255)]
    public string Adresse { get; set; } = "";

    [RegularExpression("^(|høy|middels|lav)$", ErrorMessage = "Velg en gyldig prioritet.")]
    public string Prioritet { get; set; } = "";

    [Range(typeof(DateTime), "1900-01-01", "2100-12-31")]
    public DateTime? Frist { get; set; }

    public string? Beskrivelse { get; set; }
}
