namespace azir_sempro.Data;

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Heimevernet = "Heimevernet";
    public const string Privatperson = "Privatperson";
    public const string Bedrift = "Bedrift";
    public const string AdminOnly = "AdminOnly";

    public static readonly string[] All =
    [
        Admin,
        Heimevernet,
        Privatperson,
        Bedrift
    ];
}
