using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PropertyPlatform.SharedKernel;

/// <summary>
/// A self-contained business module. Each module owns its own Postgres schema,
/// registers its own services and maps its own endpoints under /api/{Name}.
/// Modules must not reference each other directly (enforced by ArchitectureTests).
/// </summary>
public interface IModule
{
    string Name { get; }

    void RegisterServices(IServiceCollection services, IConfiguration configuration);

    void MapEndpoints(IEndpointRouteBuilder endpoints);
}
