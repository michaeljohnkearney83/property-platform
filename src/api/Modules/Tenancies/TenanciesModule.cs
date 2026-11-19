using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PropertyPlatform.SharedKernel;

namespace PropertyPlatform.Modules.Tenancies;

/// <summary>
/// Tenancies module – skeleton. Follow the Portfolio module pattern:
/// own DbContext with schema "tenancies", org query filter, endpoints under /api/tenancies.
/// </summary>
public sealed class TenanciesModule : IModule
{
    public string Name => "tenancies";

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGroup($"/api/{Name}").WithTags("Tenancies")
            .MapGet("/status", () => Results.Ok(new { module = Name, status = "not implemented yet" }));
    }
}
