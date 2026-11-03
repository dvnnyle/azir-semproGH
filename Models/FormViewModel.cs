using System.ComponentModel.DataAnnotations;

namespace azir_sempro.Models;

public class FormViewModel
{
    [StringLength(255)]
    public string Tittel { get; set; } = "";
    public int CategoryId { get; set; }
    // Hva det er: "ressurs" (tilbyr hjelp) eller "behov" (trenger hjelp) - styrer ikon.
    [Required]
    [RegularExpression("^(ressurs|behov)$")]
    public string Type { get; set; } = "ressurs";
    // Menneskelesbar adresse brukeren skriver inn selv (f.eks. "Elvegata 12, 4610 Kristiansand").
    [StringLength(255)]
    public string Adresse { get; set; } = "";
    // "høy", "middels" eller "lav" - bare for behov. Ressurs er alltid gronn pa
    // kartet (utledes fra Type), trenger ikke velges.
    [RegularExpression("^(lav|middels|høy)$")]
    public string? Prioritet { get; set; }
    [Range(typeof(DateTime), "1900-01-01", "2100-12-31")]
    public DateTime? Frist { get; set; }
    // Rakoordinater fra Leaflet (JSON-array {lat,lng}) - wire-format som sendes
    // mellom stegene. FormController.Submit() gjor dette om til Points-rader,
    // ikke lagret rått noe sted i databasen lenger.
    [StringLength(65536)]
    public string PunkterJson { get; set; } = "";
    [StringLength(4000)]
    public string Beskrivelse { get; set; } = "";
    public DateTime Tidspunkt { get; set; }
    // Statusflyt fra case.md: ny -> vurderes -> tildelt -> lost. Settes til "ny" ved
    // innsending (FormController.Submit) - endres ikke herfra enna, det krever en
    // admin/oversikt-side som ikke er bygget.
    public string Status { get; set; } = "ny";
}
