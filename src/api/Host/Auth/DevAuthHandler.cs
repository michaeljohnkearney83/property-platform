using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace PropertyPlatform.Host.Auth;

/// <summary>
/// LOCAL DEVELOPMENT ONLY. Registered only when the environment is Development
/// and no Entra authority is configured (see Program.cs). Signs every request in
/// as a fake user; the organisation comes from the X-Org-Id header or a fixed demo org.
/// </summary>
public sealed class DevAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Dev";
    public static readonly Guid DemoOrgId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var org = Request.Headers.TryGetValue("X-Org-Id", out var h) && Guid.TryParse(h, out var parsed) ? parsed : DemoOrgId;

        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, "dev-user"),
            new Claim(ClaimTypes.Name, "Dev User"),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(OrgClaims.OrgId, org.ToString()),
        ], SchemeName);

        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName)));
    }
}

public static class OrgClaims
{
    /// <summary>Custom claim issued by Entra External ID (via a custom claims provider) carrying the organisation id.</summary>
    public const string OrgId = "org_id";
}
