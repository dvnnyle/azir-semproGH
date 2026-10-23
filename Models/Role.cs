namespace azir_sempro.Models;

public class Role
{
    public int RoleId { get; set; }
    public string Navn { get; set; } = "";
    // "offentlig", "privat" eller "admin" - styrer tilgang
    public string Gruppe { get; set; } = "privat";
}
