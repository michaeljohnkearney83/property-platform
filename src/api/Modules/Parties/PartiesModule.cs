using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PropertyPlatform.SharedKernel;

namespace PropertyPlatform.Modules.Parties;

/// <summary>
/// Parties module – skeleton. Follow the Portfolio module pattern:
/// own DbContext with schema "parties", org query filter, endpoints under /api/parties.
/// </summary>
public sealed class PartiesModule : IModule
{
    public string Name => "parties";

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGroup($"/api/{Name}").WithTags("Parties")
            .MapGet("/status", () => Results.Ok(new { module = Name, status = "not implemented yet" }));
    }
}
