using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.HttpOverrides;
using System.Threading.RateLimiting;
using System.Net;
using System.Security.Cryptography;
using azir_sempro.Data;
using azir_sempro.Models;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.ForwardLimit = 1;
    foreach (var proxyAddress in builder.Configuration.GetSection("ForwardedHeaders:KnownProxies").Get<string[]>() ?? [])
    {
        if (!IPAddress.TryParse(proxyAddress, out var address))
        {
            throw new InvalidOperationException($"Invalid trusted proxy IP address: '{proxyAddress}'.");
        }

        options.KnownProxies.Add(address);
    }
});

builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(365);
    options.IncludeSubDomains = false;
    options.Preload = false;
});

//database connection
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)));

builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Sender lenke for "glemt passord". Byttes ut med en ekte e-posttjeneste ved behov.
builder.Services.AddTransient<IEmailSender<ApplicationUser>, azir_sempro.Services.DevEmailSender>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.AccessDeniedPath = "/access-denied";
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
});

builder.Services.AddAntiforgery(options =>
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always);

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("login", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
    options.AddPolicy("registration", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromHours(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});

builder.Services.AddAuthorization(options =>
    options.AddPolicy(AppRoles.AdminOnly, policy => policy.RequireRole(AppRoles.Admin)));

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
    foreach (var roleName in AppRoles.All)
    {
        if (await roleManager.RoleExistsAsync(roleName))
        {
            continue;
        }

        var result = await roleManager.CreateAsync(new IdentityRole<int>(roleName));
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"Could not create the '{roleName}' role: {errors}");
        }
    }

    // Ferdige testbrukere for lokal utvikling - kun i dev, ikke i produksjon.
    if (app.Environment.IsDevelopment())
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var testbrukere = new[]
        {
            (Epost: "admin@krisis.no", Passord: "!Krisis123", Rolle: AppRoles.Admin, Fornavn: "Admin", Etternavn: "Krisis"),
            (Epost: "bruker@heimevernet.no", Passord: "!Heimervernet123", Rolle: AppRoles.Heimevernet, Fornavn: "HV", Etternavn: "Bruker")
        };

        foreach (var (epost, passord, rolle, fornavn, etternavn) in testbrukere)
        {
            if (await userManager.FindByEmailAsync(epost) is not null)
            {
                continue;
            }

            var bruker = new ApplicationUser
            {
                UserName = epost,
                Email = epost,
                EmailConfirmed = true,
                FirstName = fornavn,
                LastName = etternavn,
                PhoneNumber = "00000000"
            };

            var brukerResultat = await userManager.CreateAsync(bruker, passord);
            if (brukerResultat.Succeeded)
            {
                await userManager.AddToRoleAsync(bruker, rolle);
            }
        }
    }
}

// Configure the HTTP request pipeline.
app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error/Error");
    app.UseHsts();
    app.Use(async (context, next) =>
    {
        if (!context.Request.IsHttps &&
            context.Request.Method is not ("GET" or "HEAD" or "OPTIONS"))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        await next();
    });
}

app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        context.Response.Headers.Remove("Server");
        context.Response.Headers.Remove("X-Powered-By");
        return Task.CompletedTask;
    });

    var nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    context.Items["CspNonce"] = nonce;

    context.Response.Headers["Content-Security-Policy"] =
        $"default-src 'self'; base-uri 'self'; object-src 'none'; frame-src 'none'; frame-ancestors 'none'; " +
        $"form-action 'self'; script-src 'self' 'nonce-{nonce}'; " +
        "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdnjs.cloudflare.com; " +
        "font-src 'self' data: https://fonts.gstatic.com https://cdnjs.cloudflare.com; " +
        "img-src 'self' data: blob: https://cache.kartverket.no; " +
        "connect-src 'self' https://ws.geonorge.no https://cache.kartverket.no https://api.kartverket.no";
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Permissions-Policy"] = "geolocation=(self), camera=(), microphone=()";

    await next();
});

app.UseRouting();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
