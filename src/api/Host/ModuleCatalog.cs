using PropertyPlatform.Modules.Compliance;
using PropertyPlatform.Modules.Documents;
using PropertyPlatform.Modules.Identity;
using PropertyPlatform.Modules.Ledger;
using PropertyPlatform.Modules.Maintenance;
using PropertyPlatform.Modules.Parties;
using PropertyPlatform.Modules.Portfolio;
using PropertyPlatform.Modules.Tenancies;
using PropertyPlatform.SharedKernel;

namespace PropertyPlatform.Host;

/// <summary>Explicit list of modules. Adding a module = new project + one line here.</summary>
public static class ModuleCatalog
{
    public static IReadOnlyList<IModule> All { get; } =
    [
        new IdentityModule(),
        new PortfolioModule(),
        new PartiesModule(),
        new TenanciesModule(),
        new LedgerModule(),
        new ComplianceModule(),
        new MaintenanceModule(),
        new DocumentsModule(),
    ];
}
