using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PropertyPlatform.SharedKernel;

namespace PropertyPlatform.Modules.Identity;

/// <summary>
/// Identity module – skeleton. Follow the Portfolio module pattern:
/// own DbContext with schema "identity", org query filter, endpoints under /api/identity.
/// </summary>
public sealed class IdentityModule : IModule
{
    public string Name => "identity";

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGroup($"/api/{Name}").WithTags("Identity")
            .MapGet("/status", () => Results.Ok(new { module = Name, status = "not implemented yet" }));
    }
}
