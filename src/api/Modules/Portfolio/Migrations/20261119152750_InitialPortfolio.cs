using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PropertyPlatform.Modules.Portfolio.Migrations
{
    /// <inheritdoc />
    public partial class InitialPortfolio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "portfolio");

            migrationBuilder.CreateTable(
                name: "properties",
                schema: "portfolio",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrgId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reference = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AddressLine1 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Town = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Postcode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Bedrooms = table.Column<int>(type: "integer", nullable: false),
                    CreatedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_properties", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_properties_OrgId_Reference",
                schema: "portfolio",
                table: "properties",
                columns: new[] { "OrgId", "Reference" },
                unique: true);

            // Row-level security: every row is visible only when app.org_id (set per connection
            // by OrgRlsConnectionInterceptor) matches. FORCE applies it to the table owner too.
            migrationBuilder.Sql("""
                ALTER TABLE portfolio.properties ENABLE ROW LEVEL SECURITY;
                ALTER TABLE portfolio.properties FORCE ROW LEVEL SECURITY;
                CREATE POLICY org_isolation ON portfolio.properties
                    USING ("OrgId" = NULLIF(current_setting('app.org_id', true), '')::uuid)
                    WITH CHECK ("OrgId" = NULLIF(current_setting('app.org_id', true), '')::uuid);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "properties",
                schema: "portfolio");
        }
    }
}
