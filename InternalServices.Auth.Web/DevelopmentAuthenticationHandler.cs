using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InternalServices.Auth.Web;

internal sealed class DevelopmentAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public DevelopmentAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var env = Context.RequestServices.GetRequiredService<IHostEnvironment>();
        if (!env.IsDevelopment())
            return Task.FromResult(AuthenticateResult.Fail("Development auth is only available in Development."));
        
        var externalId = Context.RequestServices
            .GetRequiredService<IConfiguration>()["DevAuth:ExternalId"];

        if (string.IsNullOrWhiteSpace(externalId))
        {
            return Task.FromResult(AuthenticateResult.Fail(
                "DevAuth:ExternalId is missing from configuration."));
        }

        var email = Context.RequestServices.GetRequiredService<IConfiguration>()["DevAuth:Email"] ?? "dev@local.com" ;
        var firstName = Context.RequestServices.GetRequiredService<IConfiguration>()["DevAuth:FirstName"] ?? "Dev";
        var lastName = Context.RequestServices.GetRequiredService<IConfiguration>()["DevAuth:LastName"] ?? "User";
        var username = Context.RequestServices.GetRequiredService<IConfiguration>()["DevAuth:Username"] ?? email;
        var displayName = $"{firstName} {lastName}".Trim();

        var claims = new List<Claim>
        {
            new("oid", externalId),
            new("http://schemas.microsoft.com/identity/claims/objectidentifier", externalId),
            new("preferred_username", username),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.GivenName, firstName),
            new(ClaimTypes.Surname, lastName),
            new("name", displayName)
        };

        var identity = new ClaimsIdentity(claims, DevelopmentAuthenticationDefaults.Schema);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, DevelopmentAuthenticationDefaults.Schema);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}