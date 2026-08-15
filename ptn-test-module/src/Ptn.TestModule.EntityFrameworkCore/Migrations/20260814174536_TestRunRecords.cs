using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ptn.TestModule.Migrations
{
    /// <inheritdoc />
    public partial class TestRunRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "test_run");

            migrationBuilder.CreateTable(
                name: "test_runs",
                schema: "test_run",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    scenario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    test_key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    history_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    environment_key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    spec_snapshot_id = table.Column<Guid>(type: "uuid", nullable: true),
                    db_connection_id = table.Column<Guid>(type: "uuid", nullable: true),
                    run_status_id = table.Column<Guid>(type: "uuid", nullable: false),
                    trigger_kind_id = table.Column<Guid>(type: "uuid", nullable: false),
                    trigger_ref = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    trace_id = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    spec_fingerprint = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    db_schema_fingerprint = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    runner_ref = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    is_dry_run = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    har_blob_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("pk_test_runs", x => x.id);
                    table.ForeignKey(
                        name: "fk_test_runs_test_run_statuses_run_status_id",
                        column: x => x.run_status_id,
                        principalSchema: "test_lookup",
                        principalTable: "test_run_statuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_test_runs_test_scenarios_scenario_id",
                        column: x => x.scenario_id,
                        principalSchema: "test_catalog",
                        principalTable: "test_scenarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_test_runs_test_trigger_kinds_trigger_kind_id",
                        column: x => x.trigger_kind_id,
                        principalSchema: "test_lookup",
                        principalTable: "test_trigger_kinds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "test_run_results",
                schema: "test_run",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    test_run_id = table.Column<Guid>(type: "uuid", nullable: false),
                    attempt = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    outcome_status_id = table.Column<Guid>(type: "uuid", nullable: false),
                    duration_ms = table.Column<int>(type: "integer", nullable: false),
                    failure_category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    error_code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    detail = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    failed_step_ordinal = table.Column<int>(type: "integer", nullable: true),
                    failed_step_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    failed_step_path = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    taken_branch_path = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    last_completed_ordinal = table.Column<int>(type: "integer", nullable: true),
                    diagnosis_report = table.Column<string>(type: "jsonb", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_test_run_results", x => x.id);
                    table.ForeignKey(
                        name: "fk_test_run_results_test_failure_categories_failure_category_id",
                        column: x => x.failure_category_id,
                        principalSchema: "test_lookup",
                        principalTable: "test_failure_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_test_run_results_test_outcome_statuses_outcome_status_id",
                        column: x => x.outcome_status_id,
                        principalSchema: "test_lookup",
                        principalTable: "test_outcome_statuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_test_run_results_test_runs_test_run_id",
                        column: x => x.test_run_id,
                        principalSchema: "test_run",
                        principalTable: "test_runs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "test_result_findings",
                schema: "test_run",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    test_run_result_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ordinal = table.Column<int>(type: "integer", nullable: false),
                    source_checker_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    comparison_kind_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    rule_ref = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    location = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    target_display_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    expected_value = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    observed_value = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    evidence_summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    observed_at_ms = table.Column<int>(type: "integer", nullable: true),
                    attempt_count = table.Column<int>(type: "integer", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_test_result_findings", x => x.id);
                    table.ForeignKey(
                        name: "fk_test_result_findings_test_run_results_test_run_result_id",
                        column: x => x.test_run_result_id,
                        principalSchema: "test_run",
                        principalTable: "test_run_results",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_findings_loc",
                schema: "test_run",
                table: "test_result_findings",
                column: "location");

            migrationBuilder.CreateIndex(
                name: "ix_findings_rule",
                schema: "test_run",
                table: "test_result_findings",
                column: "rule_ref");

            migrationBuilder.CreateIndex(
                name: "ix_findings_src",
                schema: "test_run",
                table: "test_result_findings",
                column: "source_checker_code");

            migrationBuilder.CreateIndex(
                name: "ux_findings_order",
                schema: "test_run",
                table: "test_result_findings",
                columns: new[] { "test_run_result_id", "ordinal" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_results_error",
                schema: "test_run",
                table: "test_run_results",
                column: "error_code");

            migrationBuilder.CreateIndex(
                name: "ix_test_run_results_failure_category_id",
                schema: "test_run",
                table: "test_run_results",
                column: "failure_category_id");

            migrationBuilder.CreateIndex(
                name: "ix_test_run_results_outcome_status_id",
                schema: "test_run",
                table: "test_run_results",
                column: "outcome_status_id");

            migrationBuilder.CreateIndex(
                name: "ux_results_attempt",
                schema: "test_run",
                table: "test_run_results",
                columns: new[] { "test_run_id", "attempt" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_runs_stale",
                schema: "test_run",
                table: "test_runs",
                columns: new[] { "run_status_id", "started_at" });

            migrationBuilder.CreateIndex(
                name: "ix_runs_test",
                schema: "test_run",
                table: "test_runs",
                column: "test_key");

            migrationBuilder.CreateIndex(
                name: "ix_runs_trace",
                schema: "test_run",
                table: "test_runs",
                column: "trace_id");

            migrationBuilder.CreateIndex(
                name: "ix_runs_trend",
                schema: "test_run",
                table: "test_runs",
                column: "history_id");

            migrationBuilder.CreateIndex(
                name: "ix_test_runs_scenario_id",
                schema: "test_run",
                table: "test_runs",
                column: "scenario_id");

            migrationBuilder.CreateIndex(
                name: "ix_test_runs_trigger_kind_id",
                schema: "test_run",
                table: "test_runs",
                column: "trigger_kind_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "test_result_findings",
                schema: "test_run");

            migrationBuilder.DropTable(
                name: "test_run_results",
                schema: "test_run");

            migrationBuilder.DropTable(
                name: "test_runs",
                schema: "test_run");
        }
    }
}
