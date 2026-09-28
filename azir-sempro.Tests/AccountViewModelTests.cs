using System.ComponentModel.DataAnnotations;
using azir_sempro.Models;

namespace azir_sempro.Tests;

public class AccountViewModelTests
{
    // Kjører valideringsreglene ([Required], [EmailAddress], [MinLength]) på modellen
    private static List<ValidationResult> Valider(AccountViewModel model)
    {
        var resultater = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), resultater, validateAllProperties: true);
        return resultater;
    }

    private static AccountViewModel GyldigModell() => new()
    {
        FirstName = "Juan",
        LastName = "Johnson",
        Email = "juan@example.com",
        PhoneNumber = "12345678",
        Password = "passord123"
    };

    [Fact]
    public void GyldigModell_GirIngenFeil()
    {
        Assert.Empty(Valider(GyldigModell()));
    }

    [Fact]
    public void UgyldigEpost_GirFeil()
    {
        var model = GyldigModell();
        model.Email = "ikke-en-epost";
        Assert.NotEmpty(Valider(model));
    }

    [Fact]
    public void KortPassord_GirFeil()
    {
        var model = GyldigModell();
        model.Password = "kort";
        Assert.NotEmpty(Valider(model));
    }

    [Fact]
    public void TomtFornavn_GirFeil()
    {
        var model = GyldigModell();
        model.FirstName = "";
        Assert.NotEmpty(Valider(model));
    }
}