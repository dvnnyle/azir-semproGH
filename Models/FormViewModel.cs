namespace azir_sempro.Models;

public class FormViewModel
{
    public string Tittel { get; set; } = "";
    public string Kategori { get; set; } = "";
    // Fargekode fra oppgavens fastsatte kart-legende (case.md): rod = akutt behov,
    // gul = planlagt behov, gronn = tilgjengelig ressurs, bla = offentlig aktor.
    // Denne flyten er ressurstilbud, sa "gronn" er standardvalget.
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
