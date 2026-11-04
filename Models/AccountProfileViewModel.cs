using System.ComponentModel.DataAnnotations;

namespace azir_sempro.Models;

public class AccountProfileViewModel
{
    public List<ProfileSubmissionViewModel> Submissions { get; set; } = [];
    public List<ProfileDraftViewModel> Drafts { get; set; } = [];
}

public class ProfileSubmissionViewModel
{
    public int SubmissionId { get; set; }
    public string Tittel { get; set; } = "";
    public string Type { get; set; } = "";
    public string Status { get; set; } = "";
    public string Adresse { get; set; } = "";
    public string CategoryName { get; set; } = "";
    public DateTime Tidspunkt { get; set; }
}

public class ProfileDraftViewModel
{
    public int DraftId { get; set; }
    public string Tittel { get; set; } = "";
    public string Type { get; set; } = "";
    public DateTime UpdatedAt { get; set; }
}

public class ProfileSubmissionEditViewModel
{
    [Required(ErrorMessage = "Skriv inn en tittel.")]
    [StringLength(255)]
    public string Tittel { get; set; } = "";

    [Range(1, int.MaxValue, ErrorMessage = "Velg en kategori.")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Skriv inn en adresse.")]
    [StringLength(255)]
    public string Adresse { get; set; } = "";

    [RegularExpression("^(|høy|middels|lav)$", ErrorMessage = "Velg en gyldig prioritet.")]
    public string Prioritet { get; set; } = "";

    [Range(typeof(DateTime), "1900-01-01", "2100-12-31")]
    public DateTime? Frist { get; set; }

    [StringLength(4000)]
    public string? Beskrivelse { get; set; }

    public string Type { get; set; } = "";
}
