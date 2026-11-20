using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using PropertyPlatform.SharedKernel.Compliance;
using PropertyPlatform.SharedKernel.Tenancy;

namespace PropertyPlatform.Tests;

/// <summary>
/// Guardrails from docs/compliance. They read the real EF models and migrations of every module,
/// so new code (human or AI-generated) can't quietly skip organisation isolation or personal-data marking.
/// </summary>
public partial class ComplianceArchitectureTests
{
    private const string NpgsqlProvider = "Npgsql.EntityFrameworkCore.PostgreSQL";

    public static TheoryData<Type> DbContextTypes()
    {
        var data = new TheoryData<Type>();
        foreach (var t in Modules.Assemblies.SelectMany(a => a.GetTypes())
                     .Where(t => typeof(DbContext).IsAssignableFrom(t) && !t.IsAbstract))
        {
            data.Add(t);
        }

        return data;
    }

    [Fact]
    public void At_least_one_module_DbContext_is_checked()
    {
        Assert.NotEmpty(DbContextTypes());
    }

    // Rule DP-01 / ORG-01: the first line of defence is an EF query filter on OrgId.
    [Theory]
    [MemberData(nameof(DbContextTypes))]
    public void Every_org_owned_entity_has_a_query_filter(Type contextType)
    {
        using var context = Create(contextType);

        var missing = OrgOwnedEntities(context.Model)
            .Where(e => e.GetDeclaredQueryFilters().Count == 0)
            .Select(e => e.ClrType.Name)
            .ToList();

        Assert.True(missing.Count == 0,
            $"{contextType.Name}: {string.Join(", ", missing)} implement IOrgOwned but have no HasQueryFilter on OrgId.");
    }

    // Rule ORG-02: the second line of defence is Postgres row-level security, created in a migration.
    [Theory]
    [MemberData(nameof(DbContextTypes))]
    public void Every_org_owned_table_has_row_level_security_in_a_migration(Type contextType)
    {
        using var context = Create(contextType);
        var sql = Normalise(string.Join("\n", MigrationSql(contextType)));

        var problems = new List<string>();
        foreach (var entity in OrgOwnedEntities(context.Model))
        {
            var table = $"{entity.GetSchema() ?? "public"}.{entity.GetTableName()}".ToLowerInvariant();
            foreach (var required in new[]
                     {
                         $"alter table {table} enable row level security",
                         $"alter table {table} force row level security",
                         $" on {table} using",
                     })
            {
                if (!sql.Contains(required, StringComparison.Ordinal))
                {
                    problems.Add($"{entity.ClrType.Name} ({table}) is missing \"{required.Trim()}\"");
                }
            }
        }

        Assert.True(problems.Count == 0,
            $"{contextType.Name}: org-owned tables need ENABLE + FORCE ROW LEVEL SECURITY and a CREATE POLICY ... USING in a migration " +
            $"(see Modules/Portfolio/Migrations/*_InitialPortfolio.cs).\n{string.Join("\n", problems)}");
    }

    // Rule DP-02: every persisted property that looks like personal data is marked, so it can be
    // excluded from logs, included in subject access exports and handled on erasure.
    [Theory]
    [MemberData(nameof(DbContextTypes))]
    public void Persisted_personal_data_is_annotated(Type contextType)
    {
        using var context = Create(contextType);

        var unmarked = context.Model.GetEntityTypes()
            .SelectMany(e => e.GetProperties())
            .Where(p => p.PropertyInfo is { } info && LooksLikePersonalData().IsMatch(p.Name)
                        && info.GetCustomAttribute<PersonalDataAttribute>() is null
                        && info.GetCustomAttribute<NotPersonalDataAttribute>() is null)
            .Select(p => $"{p.DeclaringType.ClrType.Name}.{p.Name}")
            .ToList();

        Assert.True(unmarked.Count == 0,
            $"{contextType.Name}: add [PersonalData(...)] or [NotPersonalData(\"reason\")] to {string.Join(", ", unmarked)}. " +
            "See docs/compliance/data-protection.md.");
    }

    [Theory]
    [InlineData("FirstName")]
    [InlineData("EmailAddress")]
    [InlineData("MobilePhone")]
    [InlineData("DateOfBirth")]
    [InlineData("NationalInsuranceNumber")]
    [InlineData("RightToRentShareCode")]
    [InlineData("SortCode")]
    [InlineData("Postcode")]
    [InlineData("AddressLine1")]
    public void Personal_data_name_detection_catches_common_fields(string name)
    {
        Assert.Matches(LooksLikePersonalData(), name);
    }

    [Theory]
    [InlineData("Reference")]
    [InlineData("Bedrooms")]
    [InlineData("OrgId")]
    [InlineData("CreatedUtc")]
    public void Personal_data_name_detection_ignores_ordinary_fields(string name)
    {
        Assert.DoesNotMatch(LooksLikePersonalData(), name);
    }

    [GeneratedRegex(
        "(^|[a-z])(First|Last|Middle|Given|Full|Fore|Sur|Maiden|Display)?Name$|Forename|Surname|Email|Phone|Mobile|Telephone|" +
        "DateOfBirth|BirthDate|Dob$|NationalInsurance|NiNumber|Passport|DrivingLicen[cs]e|ShareCode|" +
        "BankAccount|AccountNumber|SortCode|Iban|Address|Postcode|PostCode|IpAddress",
        RegexOptions.CultureInvariant)]
    private static partial Regex LooksLikePersonalData();

    private static IEnumerable<IEntityType> OrgOwnedEntities(IModel model) =>
        model.GetEntityTypes().Where(e => typeof(IOrgOwned).IsAssignableFrom(e.ClrType) && !e.IsOwned());

    private static IEnumerable<string> MigrationSql(Type contextType) =>
        contextType.Assembly.GetTypes()
            .Where(t => typeof(Migration).IsAssignableFrom(t) && !t.IsAbstract
                        && t.GetCustomAttribute<DbContextAttribute>()?.ContextType == contextType)
            .Select(t =>
            {
                var migration = (Migration)Activator.CreateInstance(t)!;
                migration.ActiveProvider = NpgsqlProvider;
                return migration;
            })
            .SelectMany(m => m.UpOperations.OfType<SqlOperation>())
            .Select(op => op.Sql);

    private static string Normalise(string sql) =>
        Regex.Replace(sql.Replace("\"", string.Empty, StringComparison.Ordinal), @"\s+", " ").ToLowerInvariant();

    /// <summary>Builds a module DbContext against Npgsql without opening a connection, just to read its model.</summary>
    private static DbContext Create(Type contextType)
    {
        var builder = (DbContextOptionsBuilder)Activator.CreateInstance(typeof(DbContextOptionsBuilder<>).MakeGenericType(contextType))!;
        builder.UseNpgsql("Host=localhost;Database=model-only");

        var ctor = contextType.GetConstructors().OrderByDescending(c => c.GetParameters().Length).First();
        var args = ctor.GetParameters().Select(p => p.ParameterType switch
        {
            var t when typeof(DbContextOptions).IsAssignableFrom(t) => (object)builder.Options,
            var t when t == typeof(IOrgContext) => new OrgContext(),
            var t => throw new InvalidOperationException($"{contextType.Name} needs a {t.Name}; teach ComplianceArchitectureTests how to build it."),
        }).ToArray();

        return (DbContext)ctor.Invoke(args);
    }
}
