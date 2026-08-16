using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ptn.DatabaseChecker.Migrations
{
    /// <inheritdoc />
    public partial class InitialDatabaseCheckerModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "lookup");

            migrationBuilder.EnsureSchema(
                name: "definition");

            migrationBuilder.EnsureSchema(
                name: "run");

            migrationBuilder.EnsureSchema(
                name: "connection");

            migrationBuilder.CreateTable(
                name: "comparison_confidences",
                schema: "lookup",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_comparison_confidences", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "comparison_run_statuses",
                schema: "lookup",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_comparison_run_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "comparison_types",
                schema: "lookup",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_comparison_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "database_engines",
                schema: "lookup",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_database_engines", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "difference_kinds",
                schema: "lookup",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_difference_kinds", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "report_formats",
                schema: "lookup",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_report_formats", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "schema_object_types",
                schema: "lookup",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_schema_object_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "scope_kinds",
                schema: "lookup",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_scope_kinds", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "database_connections",
                schema: "connection",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    engine_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    host = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    port = table.Column<int>(type: "integer", nullable: false),
                    database_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    vault_secret_path = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("pk_database_connections", x => x.id);
                    table.ForeignKey(
                        name: "fk_database_connections_database_engines_engine_id",
                        column: x => x.engine_id,
                        principalSchema: "lookup",
                        principalTable: "database_engines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "comparison_definitions",
                schema: "definition",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    source_connection_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_connection_id = table.Column<Guid>(type: "uuid", nullable: false),
                    comparison_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("pk_comparison_definitions", x => x.id);
                    table.ForeignKey(
                        name: "fk_comparison_definitions_comparison_types_comparison_type_id",
                        column: x => x.comparison_type_id,
                        principalSchema: "lookup",
                        principalTable: "comparison_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_comparison_definitions_database_connections_source_connecti",
                        column: x => x.source_connection_id,
                        principalSchema: "connection",
                        principalTable: "database_connections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_comparison_definitions_database_connections_target_connecti",
                        column: x => x.target_connection_id,
                        principalSchema: "connection",
                        principalTable: "database_connections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "comparison_runs",
                schema: "run",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    comparison_definition_id = table.Column<Guid>(type: "uuid", nullable: true),
                    source_connection_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_connection_id = table.Column<Guid>(type: "uuid", nullable: false),
                    comparison_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status_id = table.Column<Guid>(type: "uuid", nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error_message = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    schema_difference_count = table.Column<int>(type: "integer", nullable: false),
                    data_difference_count = table.Column<int>(type: "integer", nullable: false),
                    migration_difference_count = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    findings = table.Column<string>(type: "jsonb", nullable: false),
                    reports = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_comparison_runs", x => x.id);
                    table.ForeignKey(
                        name: "fk_comparison_runs_comparison_definitions_comparison_definitio",
                        column: x => x.comparison_definition_id,
                        principalSchema: "definition",
                        principalTable: "comparison_definitions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_comparison_runs_comparison_run_statuses_status_id",
                        column: x => x.status_id,
                        principalSchema: "lookup",
                        principalTable: "comparison_run_statuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_comparison_runs_comparison_types_comparison_type_id",
                        column: x => x.comparison_type_id,
                        principalSchema: "lookup",
                        principalTable: "comparison_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_comparison_runs_database_connections_source_connection_id",
                        column: x => x.source_connection_id,
                        principalSchema: "connection",
                        principalTable: "database_connections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_comparison_runs_database_connections_target_connection_id",
                        column: x => x.target_connection_id,
                        principalSchema: "connection",
                        principalTable: "database_connections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_comparison_confidences_code",
                schema: "lookup",
                table: "comparison_confidences",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_comparison_definitions_comparison_type_id",
                schema: "definition",
                table: "comparison_definitions",
                column: "comparison_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_comparison_definitions_creator_id_name",
                schema: "definition",
                table: "comparison_definitions",
                columns: new[] { "CreatorId", "name" },
                unique: true,
                filter: "\"TenantId\" IS NULL")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "ix_comparison_definitions_source_connection_id",
                schema: "definition",
                table: "comparison_definitions",
                column: "source_connection_id");

            migrationBuilder.CreateIndex(
                name: "ix_comparison_definitions_target_connection_id",
                schema: "definition",
                table: "comparison_definitions",
                column: "target_connection_id");

            migrationBuilder.CreateIndex(
                name: "ix_comparison_definitions_tenant_id_name",
                schema: "definition",
                table: "comparison_definitions",
                columns: new[] { "TenantId", "name" },
                unique: true,
                filter: "\"TenantId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_comparison_run_statuses_code",
                schema: "lookup",
                table: "comparison_run_statuses",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_comparison_runs_comparison_definition_id",
                schema: "run",
                table: "comparison_runs",
                column: "comparison_definition_id");

            migrationBuilder.CreateIndex(
                name: "ix_comparison_runs_comparison_type_id",
                schema: "run",
                table: "comparison_runs",
                column: "comparison_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_comparison_runs_creation_time",
                schema: "run",
                table: "comparison_runs",
                column: "CreationTime");

            migrationBuilder.CreateIndex(
                name: "ix_comparison_runs_source_connection_id",
                schema: "run",
                table: "comparison_runs",
                column: "source_connection_id");

            migrationBuilder.CreateIndex(
                name: "ix_comparison_runs_status_id",
                schema: "run",
                table: "comparison_runs",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "ix_comparison_runs_target_connection_id",
                schema: "run",
                table: "comparison_runs",
                column: "target_connection_id");

            migrationBuilder.CreateIndex(
                name: "ix_comparison_types_code",
                schema: "lookup",
                table: "comparison_types",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_database_connections_creator_id_name",
                schema: "connection",
                table: "database_connections",
                columns: new[] { "CreatorId", "name" },
                unique: true,
                filter: "\"TenantId\" IS NULL")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "ix_database_connections_engine_id",
                schema: "connection",
                table: "database_connections",
                column: "engine_id");

            migrationBuilder.CreateIndex(
                name: "ix_database_connections_tenant_id_name",
                schema: "connection",
                table: "database_connections",
                columns: new[] { "TenantId", "name" },
                unique: true,
                filter: "\"TenantId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_database_engines_code",
                schema: "lookup",
                table: "database_engines",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_difference_kinds_code",
                schema: "lookup",
                table: "difference_kinds",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_report_formats_code",
                schema: "lookup",
                table: "report_formats",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_schema_object_types_code",
                schema: "lookup",
                table: "schema_object_types",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_scope_kinds_code",
                schema: "lookup",
                table: "scope_kinds",
                column: "code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "comparison_confidences",
                schema: "lookup");

            migrationBuilder.DropTable(
                name: "comparison_runs",
                schema: "run");

            migrationBuilder.DropTable(
                name: "difference_kinds",
                schema: "lookup");

            migrationBuilder.DropTable(
                name: "report_formats",
                schema: "lookup");

            migrationBuilder.DropTable(
                name: "schema_object_types",
                schema: "lookup");

            migrationBuilder.DropTable(
                name: "scope_kinds",
                schema: "lookup");

            migrationBuilder.DropTable(
                name: "comparison_definitions",
                schema: "definition");

            migrationBuilder.DropTable(
                name: "comparison_run_statuses",
                schema: "lookup");

            migrationBuilder.DropTable(
                name: "comparison_types",
                schema: "lookup");

            migrationBuilder.DropTable(
                name: "database_connections",
                schema: "connection");

            migrationBuilder.DropTable(
                name: "database_engines",
                schema: "lookup");
        }
    }
}
