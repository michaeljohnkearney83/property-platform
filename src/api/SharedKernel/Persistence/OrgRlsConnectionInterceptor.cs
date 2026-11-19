using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PropertyPlatform.SharedKernel.Tenancy;

namespace PropertyPlatform.SharedKernel.Persistence;

/// <summary>
/// Sets the Postgres session variable <c>app.org_id</c> every time a connection is opened,
/// so row-level security policies (see /db/rls.sql) restrict every query to the current organisation.
/// This is the second line of defence; EF global query filters are the first.
/// Always set (even to empty) so a pooled connection never carries a previous request's org.
/// </summary>
public sealed class OrgRlsConnectionInterceptor(IOrgContext orgContext) : DbConnectionInterceptor
{
    private const string Sql = "SELECT set_config('app.org_id', @org, false)";

    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        using var cmd = CreateCommand(connection);
        cmd.ExecuteNonQuery();
    }

    public override async Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
    {
        await using var cmd = CreateCommand(connection);
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private DbCommand CreateCommand(DbConnection connection)
    {
        var cmd = connection.CreateCommand();
        cmd.CommandText = Sql;
        var p = cmd.CreateParameter();
        p.ParameterName = "org";
        p.Value = orgContext.OrgId?.ToString() ?? string.Empty;
        cmd.Parameters.Add(p);
        return cmd;
    }
}
