using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PropertyPlatform.SharedKernel;

namespace PropertyPlatform.Modules.Ledger;

/// <summary>
/// Ledger module – skeleton. Follow the Portfolio module pattern:
/// own DbContext with schema "ledger", org query filter, endpoints under /api/ledger.
/// </summary>
public sealed class LedgerModule : IModule
{
    public string Name => "ledger";

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGroup($"/api/{Name}").WithTags("Ledger")
            .MapGet("/status", () => Results.Ok(new { module = Name, status = "not implemented yet" }));
    }
}
