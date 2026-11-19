using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PropertyPlatform.SharedKernel;
using PropertyPlatform.SharedKernel.Persistence;
using PropertyPlatform.SharedKernel.Tenancy;

namespace PropertyPlatform.Modules.Portfolio;

/// <summary>Reference module: shows the pattern every other module should follow.</summary>
public sealed class PortfolioModule : IModule
{
    public string Name => "portfolio";

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<PortfolioDbContext>((sp, options) =>
            options.UseNpgsql(
                    configuration.GetConnectionString("Platform"),
                    npgsql => npgsql.MigrationsHistoryTable("__ef_migrations", PortfolioDbContext.Schema))
                .AddInterceptors(sp.GetRequiredService<OrgRlsConnectionInterceptor>()));
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup($"/api/{Name}/properties").WithTags("Portfolio");

        group.MapGet("/", async (PortfolioDbContext db, CancellationToken ct) =>
            await db.Properties
                .OrderBy(p => p.Reference)
                .Select(p => new PropertyDto(p.Id, p.Reference, p.AddressLine1, p.Town, p.Postcode, p.Type, p.Bedrooms))
                .ToListAsync(ct));

        group.MapGet("/{id:guid}", async (Guid id, PortfolioDbContext db, CancellationToken ct) =>
            await db.Properties.FindAsync([id], ct) is { } p
                ? Results.Ok(new PropertyDto(p.Id, p.Reference, p.AddressLine1, p.Town, p.Postcode, p.Type, p.Bedrooms))
                : Results.NotFound());

        group.MapPost("/", async (CreatePropertyRequest req, PortfolioDbContext db, IOrgContext org, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(req.Reference) || string.IsNullOrWhiteSpace(req.Postcode))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["reference/postcode"] = ["Reference and postcode are required."],
                });
            }

            var property = new Property(org.RequiredOrgId, req.Reference.Trim(), req.AddressLine1.Trim(), req.Town.Trim(), req.Postcode.Trim().ToUpperInvariant(), req.Type, req.Bedrooms);
            db.Properties.Add(property);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/portfolio/properties/{property.Id}", new PropertyDto(property.Id, property.Reference, property.AddressLine1, property.Town, property.Postcode, property.Type, property.Bedrooms));
        });
    }
}

public sealed record PropertyDto(Guid Id, string Reference, string AddressLine1, string Town, string Postcode, PropertyType Type, int Bedrooms);

public sealed record CreatePropertyRequest(string Reference, string AddressLine1, string Town, string Postcode, PropertyType Type, int Bedrooms);
