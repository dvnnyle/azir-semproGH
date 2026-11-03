using System.ComponentModel.DataAnnotations;

namespace azir_sempro.Models;

public class LoginViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    [StringLength(100)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [StringLength(2048)]
    public string? ReturnUrl { get; set; }
}
