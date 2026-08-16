using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ptn.DatabaseChecker.Migrations
{
    /// <inheritdoc />
    public partial class KBP702TargetConnectionSafety : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "tls_mode_code",
                schema: "connection",
                table: "database_connections",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "Require");

            migrationBuilder.AddColumn<bool>(
                name: "trust_server_certificate",
                schema: "connection",
                table: "database_connections",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "tls_mode_code",
                schema: "connection",
                table: "database_connections");

            migrationBuilder.DropColumn(
                name: "trust_server_certificate",
                schema: "connection",
                table: "database_connections");
        }
    }
}
