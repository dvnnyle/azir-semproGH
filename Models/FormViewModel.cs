namespace azir_sempro.Models;

public class FormViewModel
{
    public string Tittel { get; set; } = "";
    public string Kategori { get; set; } = "";
    // Hva det er: "ressurs" (tilbyr hjelp) eller "behov" (trenger hjelp) - styrer ikon.
    public string Type { get; set; } = "ressurs";
    // Hvor viktig det er: rod (akutt), gul (planlagt) eller gronn (lav prioritet/tilgjengelig).
    public string Farge { get; set; } = "gronn";
    public string Lokasjon { get; set; } = "";
    public string PunkterJson { get; set; } = "";
    public string Beskrivelse { get; set; } = "";
    public DateTime Tidspunkt { get; set; }
    // Statusflyt fra case.md: ny -> vurderes -> tildelt -> lost. Settes til "ny" ved
    // innsending (FormController.Submit) - endres ikke herfra enna, det krever en
    // admin/oversikt-side som ikke er bygget.
    public string Status { get; set; } = "ny";
}
