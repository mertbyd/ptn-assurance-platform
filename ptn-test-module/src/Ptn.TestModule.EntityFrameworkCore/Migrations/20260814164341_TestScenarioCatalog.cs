using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ptn.TestModule.Migrations
{
    /// <inheritdoc />
    public partial class TestScenarioCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "test_catalog");

            migrationBuilder.CreateTable(
                name: "test_scenarios",
                schema: "test_catalog",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    scenario_key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    version_no = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    state_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_document = table.Column<string>(type: "text", nullable: false),
                    source_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    compiled_document = table.Column<string>(type: "text", nullable: false),
                    compiled_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    rules_fingerprint = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    spec_snapshot_id = table.Column<Guid>(type: "uuid", nullable: true),
                    spec_fingerprint = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    db_connection_id = table.Column<Guid>(type: "uuid", nullable: true),
                    db_schema_fingerprint = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    profile_fingerprint = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    assertion_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    derivability_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    authored_by_agent = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    agent_model_ref = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    approved_by = table.Column<Guid>(type: "uuid", nullable: true),
                    approved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    approval_bound_to_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_test_scenarios", x => x.id);
                    table.ForeignKey(
                        name: "fk_test_scenarios_test_scenario_states_state_id",
                        column: x => x.state_id,
                        principalSchema: "test_lookup",
                        principalTable: "test_scenario_states",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_scenarios_state",
                schema: "test_catalog",
                table: "test_scenarios",
                column: "state_id");

            migrationBuilder.CreateIndex(
                name: "ux_scenarios_content",
                schema: "test_catalog",
                table: "test_scenarios",
                columns: new[] { "scenario_key", "source_hash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_scenarios_version",
                schema: "test_catalog",
                table: "test_scenarios",
                columns: new[] { "scenario_key", "version_no" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "test_scenarios",
                schema: "test_catalog");
        }
    }
}
