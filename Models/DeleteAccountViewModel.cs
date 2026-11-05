using System.ComponentModel.DataAnnotations;

namespace azir_sempro.Models;

// Brukeren må skrive inn passordet sitt for å bekrefte at kontoen skal slettes.
public class DeleteAccountViewModel
{
    [Required(ErrorMessage = "Skriv inn passordet ditt for å bekrefte.")]
    [StringLength(100)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";
}
