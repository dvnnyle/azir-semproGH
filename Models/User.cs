namespace azir_sempro.Models;

public class User
{
    public int UserId { get; set; }

    public string FirstName { get; set; } = "";

    public string LastName { get; set; } = "";

    public string Phone { get; set; } = "";

    public string PasswordHash { get; set; } = "";

    public string Email { get; set; } = "";
}
