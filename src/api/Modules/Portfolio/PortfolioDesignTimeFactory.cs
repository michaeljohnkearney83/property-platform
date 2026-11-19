using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using PropertyPlatform.SharedKernel.Tenancy;

namespace PropertyPlatform.Modules.Portfolio;

/// <summary>
/// Used by `dotnet ef` and migration bundles so they don't need to boot the whole API host.
/// Connection string comes from ConnectionStrings__Platform (or the bundle's --connection argument).
/// </summary>
public sealed class PortfolioDesignTimeFactory : IDesignTimeDbContextFactory<PortfolioDbContext>
{
    public PortfolioDbContext CreateDbContext(string[] args)
    {
        var cs = Environment.GetEnvironmentVariable("ConnectionStrings__Platform")
            ?? "Host=localhost;Port=5432;Database=propertyplatform;Username=app_user;Password=app_dev_password";

        var options = new DbContextOptionsBuilder<PortfolioDbContext>()
            .UseNpgsql(cs, npgsql => npgsql.MigrationsHistoryTable("__ef_migrations", PortfolioDbContext.Schema))
            .Options;

        return new PortfolioDbContext(options, new OrgContext());
    }
}
