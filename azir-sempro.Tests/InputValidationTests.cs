using System.ComponentModel.DataAnnotations;
using azir_sempro.Models;

namespace azir_sempro.Tests;

public class InputValidationTests
{
    private static List<ValidationResult> Validate(object model)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);
        return results;
    }

    [Fact]
    public void Registration_RejectsInvalidPhoneNumber()
    {
        var model = new AccountViewModel
        {
            FirstName = "Ada",
            LastName = "Lovelace",
            Email = "ada@example.com",
            PhoneNumber = "not-a-phone",
            Password = "StrongPass123",
            ConfirmPassword = "StrongPass123"
        };

        Assert.Contains(Validate(model), error => error.MemberNames.Contains(nameof(model.PhoneNumber)));
    }

    [Fact]
    public void Form_RejectsOverlongTitle()
    {
        var model = new FormViewModel { Tittel = new string('a', 256) };

        Assert.Contains(Validate(model), error => error.MemberNames.Contains(nameof(model.Tittel)));
    }

    [Fact]
    public void Form_RejectsUnknownSubmissionType()
    {
        var model = new FormViewModel { Type = "admin" };

        Assert.Contains(Validate(model), error => error.MemberNames.Contains(nameof(model.Type)));
    }

    [Fact]
    public void Form_RejectsDeadlineOutsideAllowedRange()
    {
        var model = new FormViewModel { Frist = new DateTime(2200, 1, 1) };

        Assert.Contains(Validate(model), error => error.MemberNames.Contains(nameof(model.Frist)));
    }

    [Fact]
    public void ResourceFilters_RejectUnknownStatus()
    {
        var model = new ResourceListViewModel { Status = "admin" };

        Assert.Contains(Validate(model), error => error.MemberNames.Contains(nameof(model.Status)));
    }
}
