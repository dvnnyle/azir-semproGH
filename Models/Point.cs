using Microsoft.EntityFrameworkCore;

namespace azir_sempro.Models;

// Erstatter gamle Submission.PunkterJson - en rad per hjornepunkt i stedet
// for en hel liste pakket i en kolonne.
[PrimaryKey(nameof(SubmissionId), nameof(Nr))]
public class Point
{
    public int SubmissionId { get; set; }
    // Rekkefolgen punktet ble tegnet i - avgjor formen nar polygonet tegnes opp igjen
    public short Nr { get; set; }
    public decimal Breddegrad { get; set; }
    public decimal Lengdegrad { get; set; }
}
