using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ptn.DatabaseChecker.Migrations
{
    /// <inheritdoc />
    public partial class KBP706ComparisonDefinitionSourceRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "source_role_code",
                schema: "definition",
                table: "comparison_definitions",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "Reference");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "source_role_code",
                schema: "definition",
                table: "comparison_definitions");
        }
    }
}
