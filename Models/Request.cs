namespace azir_sempro.Models;

// Kobler en ressurs-innsending til en behov-innsending - m:n lost med egen
// tabell, siden begge sider peker til samme Submissions-tabell.
public class Request
{
    public int RequestId { get; set; }
    public int BehovId { get; set; }
    public int RessursId { get; set; }
    // "sendt", "godtatt" eller "avslatt"
    public string Status { get; set; } = "sendt";
    public string? Melding { get; set; }
    public DateTime Tidspunkt { get; set; }
}
