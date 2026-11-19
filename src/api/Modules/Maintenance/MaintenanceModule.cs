using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PropertyPlatform.SharedKernel;

namespace PropertyPlatform.Modules.Maintenance;

/// <summary>
/// Maintenance module – skeleton. Follow the Portfolio module pattern:
/// own DbContext with schema "maintenance", org query filter, endpoints under /api/maintenance.
/// </summary>
public sealed class MaintenanceModule : IModule
{
    public string Name => "maintenance";

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGroup($"/api/{Name}").WithTags("Maintenance")
            .MapGet("/status", () => Results.Ok(new { module = Name, status = "not implemented yet" }));
    }
}
