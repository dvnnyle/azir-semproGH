using System.ComponentModel.DataAnnotations;

namespace azir_sempro.Models;

public class AccountViewModel
{
    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = "";

    [Required]
    [StringLength(50)]
    public string LastName { get; set; } = "";

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = "";

    [Required]
    [StringLength(15)]
    public string PhoneNumber { get; set; } = "";

    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = "";

    [Required]
    [Compare("Password")]
    public string ConfirmPassword { get; set; } = "";
}
