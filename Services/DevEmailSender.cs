using Microsoft.AspNetCore.Identity;
using azir_sempro.Models;

namespace azir_sempro.Services;

// Prosjektet har ingen e-posttjeneste ennå. Denne klassen implementerer Identitys
// IEmailSender, så en ekte e-posttjeneste (SMTP o.l.) kan byttes inn i Program.cs
// uten å endre AccountController.
// I Development skrives lenken til applikasjonsloggen (docker compose logs app),
// slik at flyten kan testes lokalt. I andre miljøer logges ikke lenken.
public class DevEmailSender(ILogger<DevEmailSender> logger, IWebHostEnvironment environment)
    : IEmailSender<ApplicationUser>
{
    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
    {
        if (environment.IsDevelopment())
        {
            logger.LogInformation("[DEV] Lenke for å tilbakestille passord for {Email}: {ResetLink}", email, resetLink);
        }
        else
        {
            logger.LogWarning("E-post er ikke satt opp. Tilbakestillingslenke ble ikke sendt.");
        }
        return Task.CompletedTask;
    }

    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
        => Task.CompletedTask;

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
        => Task.CompletedTask;
}
