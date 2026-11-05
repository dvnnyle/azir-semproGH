using System.ComponentModel.DataAnnotations;

namespace azir_sempro.Models;

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "Skriv inn e-postadressen din.")]
    [EmailAddress(ErrorMessage = "Ugyldig e-postadresse.")]
    [StringLength(256)]
    public string Email { get; set; } = "";
}

public class ResetPasswordViewModel
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = "";

    // Engangskoden fra lenken (Base64Url-kodet Identity-token).
    [Required]
    [StringLength(2048)]
    public string Code { get; set; } = "";

    [Required(ErrorMessage = "Skriv inn et nytt passord.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Passordet må ha minst 8 tegn.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "Gjenta passordet.")]
    [StringLength(100)]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Passordene er ikke like.")]
    public string ConfirmPassword { get; set; } = "";
}
