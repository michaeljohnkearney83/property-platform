namespace PropertyPlatform.SharedKernel.Tenancy;

/// <summary>
/// The SaaS customer ("Organisation") the current request belongs to.
/// Named Organisation, never "tenant", to avoid clashing with renters.
/// </summary>
public interface IOrgContext
{
    Guid? OrgId { get; }

    Guid RequiredOrgId => OrgId ?? throw new InvalidOperationException("No organisation in the current context.");
}

public sealed class OrgContext : IOrgContext
{
    public Guid? OrgId { get; private set; }

    public void Set(Guid orgId) => OrgId = orgId;
}

/// <summary>Marks an entity that belongs to exactly one organisation.</summary>
public interface IOrgOwned
{
    Guid OrgId { get; }
}
