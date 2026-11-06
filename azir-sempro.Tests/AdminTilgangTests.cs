using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using azir_sempro.Controllers;
using azir_sempro.Data;

namespace azir_sempro.Tests;

// Sjekker at adminsidene er låst. Hvis noen fjerner [Authorize] ved et uhell, feiler testen.
public class AdminTilgangTests
{
    [Fact]
    public void AdminSidene_KreverAdminEllerHeimevernet()
    {
        var attributt = typeof(AdminController).GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(attributt);
        Assert.Contains(AppRoles.Admin, attributt.Roles!);
        Assert.Contains(AppRoles.Heimevernet, attributt.Roles!);
        Assert.DoesNotContain(AppRoles.Privatperson, attributt.Roles!);
    }
}
