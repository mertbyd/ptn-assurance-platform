using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ptn.TestModule.Migrations
{
    /// <inheritdoc />
    public partial class Initial_TestModuleSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "test_lookup");

            migrationBuilder.CreateTable(
                name: "test_failure_categories",
                schema: "test_lookup",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_test_failure_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "test_outcome_statuses",
                schema: "test_lookup",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    breaks_build = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_test_outcome_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "test_run_statuses",
                schema: "test_lookup",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_test_run_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "test_scenario_states",
                schema: "test_lookup",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_test_scenario_states", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "test_trigger_kinds",
                schema: "test_lookup",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_test_trigger_kinds", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_test_failure_categories_code",
                schema: "test_lookup",
                table: "test_failure_categories",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_test_outcome_statuses_code",
                schema: "test_lookup",
                table: "test_outcome_statuses",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_test_run_statuses_code",
                schema: "test_lookup",
                table: "test_run_statuses",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_test_scenario_states_code",
                schema: "test_lookup",
                table: "test_scenario_states",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_test_trigger_kinds_code",
                schema: "test_lookup",
                table: "test_trigger_kinds",
                column: "code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "test_failure_categories",
                schema: "test_lookup");

            migrationBuilder.DropTable(
                name: "test_outcome_statuses",
                schema: "test_lookup");

            migrationBuilder.DropTable(
                name: "test_run_statuses",
                schema: "test_lookup");

            migrationBuilder.DropTable(
                name: "test_scenario_states",
                schema: "test_lookup");

            migrationBuilder.DropTable(
                name: "test_trigger_kinds",
                schema: "test_lookup");
        }
    }
}
