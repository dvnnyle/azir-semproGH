using System.ComponentModel.DataAnnotations;

namespace azir_sempro.Models;

public class AccountViewModel
{
    [Required]
    public string FirstName { get; set; } = "";

    [Required]
    public string LastName { get; set; } = "";

    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    public string PhoneNumber { get; set; } = "";

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = "";
}