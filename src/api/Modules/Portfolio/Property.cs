using Microsoft.EntityFrameworkCore;
using PropertyPlatform.SharedKernel.Tenancy;

namespace PropertyPlatform.Modules.Portfolio;

public enum PropertyType
{
    House,
    Flat,
    Hmo,
    Bungalow,
    Commercial,
}

public sealed class Property : IOrgOwned
{
    // For EF
    private Property()
    {
        Reference = AddressLine1 = Town = Postcode = string.Empty;
    }

    public Property(Guid orgId, string reference, string addressLine1, string town, string postcode, PropertyType type, int bedrooms)
    {
        Id = Guid.CreateVersion7();
        OrgId = orgId;
        Reference = reference;
        AddressLine1 = addressLine1;
        Town = town;
        Postcode = postcode;
        Type = type;
        Bedrooms = bedrooms;
        CreatedUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid OrgId { get; private set; }
    public string Reference { get; private set; }
    public string AddressLine1 { get; private set; }
    public string Town { get; private set; }
    public string Postcode { get; private set; }
    public PropertyType Type { get; private set; }
    public int Bedrooms { get; private set; }
    public DateTimeOffset CreatedUtc { get; private set; }
}

public sealed class PortfolioDbContext(DbContextOptions<PortfolioDbContext> options, IOrgContext orgContext) : DbContext(options)
{
    public const string Schema = "portfolio";

    public DbSet<Property> Properties => Set<Property>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);

        modelBuilder.Entity<Property>(e =>
        {
            e.ToTable("properties");
            e.HasKey(p => p.Id);
            e.Property(p => p.Reference).HasMaxLength(50).IsRequired();
            e.Property(p => p.AddressLine1).HasMaxLength(200).IsRequired();
            e.Property(p => p.Town).HasMaxLength(100).IsRequired();
            e.Property(p => p.Postcode).HasMaxLength(10).IsRequired();
            e.Property(p => p.Type).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(p => new { p.OrgId, p.Reference }).IsUnique();

            // First line of defence; Postgres RLS is the second (see /db/rls.sql).
            e.HasQueryFilter(p => p.OrgId == orgContext.OrgId);
        });
    }
}
