using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ptn.ApiContractChecker.Migrations
{
    /// <inheritdoc />
    public partial class InitialApiContractCheckerModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "checker");

            migrationBuilder.CreateTable(
                name: "check_run_statuses",
                schema: "checker",
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
                    table.PrimaryKey("pk_check_run_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "difference_directions",
                schema: "checker",
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
                    table.PrimaryKey("pk_difference_directions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "difference_kinds",
                schema: "checker",
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
                name: "difference_severities",
                schema: "checker",
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
                    table.PrimaryKey("pk_difference_severities", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "spec_contents",
                schema: "checker",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    raw_hash = table.Column<string>(type: "character(64)", maxLength: 64, nullable: false),
                    canonical_hash = table.Column<string>(type: "character(64)", maxLength: 64, nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    byte_size = table.Column<int>(type: "integer", nullable: false),
                    media_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_spec_contents", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "spec_formats",
                schema: "checker",
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
                    table.PrimaryKey("pk_spec_formats", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "spec_sources",
                schema: "checker",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    base_url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    vault_secret_path = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
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
                    table.PrimaryKey("pk_spec_sources", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "spec_documents",
                schema: "checker",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    spec_source_id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    path = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_monitored = table.Column<bool>(type: "boolean", nullable: false),
                    check_interval_minutes = table.Column<int>(type: "integer", nullable: true),
                    next_check_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_checked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_fetch_outcome = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_spec_documents", x => x.id);
                    table.ForeignKey(
                        name: "fk_spec_documents_spec_sources_spec_source_id",
                        column: x => x.spec_source_id,
                        principalSchema: "checker",
                        principalTable: "spec_sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "spec_snapshots",
                schema: "checker",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    spec_document_id = table.Column<Guid>(type: "uuid", nullable: false),
                    spec_content_id = table.Column<Guid>(type: "uuid", nullable: false),
                    spec_format_id = table.Column<Guid>(type: "uuid", nullable: false),
                    api_version = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    last_seen_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_spec_snapshots", x => x.id);
                    table.ForeignKey(
                        name: "fk_spec_snapshots_spec_contents_spec_content_id",
                        column: x => x.spec_content_id,
                        principalSchema: "checker",
                        principalTable: "spec_contents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_spec_snapshots_spec_documents_spec_document_id",
                        column: x => x.spec_document_id,
                        principalSchema: "checker",
                        principalTable: "spec_documents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_spec_snapshots_spec_formats_spec_format_id",
                        column: x => x.spec_format_id,
                        principalSchema: "checker",
                        principalTable: "spec_formats",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "contract_check_runs",
                schema: "checker",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    base_snapshot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_snapshot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    check_run_status_id = table.Column<Guid>(type: "uuid", nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error_message = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    breaking_count = table.Column<int>(type: "integer", nullable: false),
                    non_breaking_count = table.Column<int>(type: "integer", nullable: false),
                    docs_only_count = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    findings = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contract_check_runs", x => x.id);
                    table.ForeignKey(
                        name: "fk_contract_check_runs_check_run_statuses_check_run_status_id",
                        column: x => x.check_run_status_id,
                        principalSchema: "checker",
                        principalTable: "check_run_statuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_contract_check_runs_spec_snapshots_base_snapshot_id",
                        column: x => x.base_snapshot_id,
                        principalSchema: "checker",
                        principalTable: "spec_snapshots",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_contract_check_runs_spec_snapshots_target_snapshot_id",
                        column: x => x.target_snapshot_id,
                        principalSchema: "checker",
                        principalTable: "spec_snapshots",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_check_run_statuses_code",
                schema: "checker",
                table: "check_run_statuses",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_contract_check_runs_base_snapshot_id",
                schema: "checker",
                table: "contract_check_runs",
                column: "base_snapshot_id");

            migrationBuilder.CreateIndex(
                name: "ix_contract_check_runs_check_run_status_id",
                schema: "checker",
                table: "contract_check_runs",
                column: "check_run_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_contract_check_runs_target_snapshot_id",
                schema: "checker",
                table: "contract_check_runs",
                column: "target_snapshot_id");

            migrationBuilder.CreateIndex(
                name: "ix_contract_check_runs_tenant_id_creation_time",
                schema: "checker",
                table: "contract_check_runs",
                columns: new[] { "TenantId", "CreationTime" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_contract_check_runs_tenant_id_target_snapshot_id",
                schema: "checker",
                table: "contract_check_runs",
                columns: new[] { "TenantId", "target_snapshot_id" });

            migrationBuilder.CreateIndex(
                name: "ix_difference_directions_code",
                schema: "checker",
                table: "difference_directions",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_difference_kinds_code",
                schema: "checker",
                table: "difference_kinds",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_difference_severities_code",
                schema: "checker",
                table: "difference_severities",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_spec_contents_tenant_id_canonical_hash",
                schema: "checker",
                table: "spec_contents",
                columns: new[] { "TenantId", "canonical_hash" });

            migrationBuilder.CreateIndex(
                name: "ix_spec_contents_tenant_id_raw_hash",
                schema: "checker",
                table: "spec_contents",
                columns: new[] { "TenantId", "raw_hash" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "ix_spec_documents_is_monitored_next_check_at",
                schema: "checker",
                table: "spec_documents",
                columns: new[] { "is_monitored", "next_check_at" });

            migrationBuilder.CreateIndex(
                name: "ix_spec_documents_spec_source_id_document_name",
                schema: "checker",
                table: "spec_documents",
                columns: new[] { "spec_source_id", "document_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_spec_formats_code",
                schema: "checker",
                table: "spec_formats",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_spec_snapshots_spec_content_id",
                schema: "checker",
                table: "spec_snapshots",
                column: "spec_content_id");

            migrationBuilder.CreateIndex(
                name: "ix_spec_snapshots_spec_document_id",
                schema: "checker",
                table: "spec_snapshots",
                column: "spec_document_id");

            migrationBuilder.CreateIndex(
                name: "ix_spec_snapshots_spec_format_id",
                schema: "checker",
                table: "spec_snapshots",
                column: "spec_format_id");

            migrationBuilder.CreateIndex(
                name: "ix_spec_snapshots_tenant_id_spec_document_id_creation_time",
                schema: "checker",
                table: "spec_snapshots",
                columns: new[] { "TenantId", "spec_document_id", "CreationTime" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "ix_spec_sources_creator_id_name",
                schema: "checker",
                table: "spec_sources",
                columns: new[] { "CreatorId", "name" },
                unique: true,
                filter: "\"TenantId\" IS NULL")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "ix_spec_sources_tenant_id_name",
                schema: "checker",
                table: "spec_sources",
                columns: new[] { "TenantId", "name" },
                unique: true,
                filter: "\"TenantId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "contract_check_runs",
                schema: "checker");

            migrationBuilder.DropTable(
                name: "difference_directions",
                schema: "checker");

            migrationBuilder.DropTable(
                name: "difference_kinds",
                schema: "checker");

            migrationBuilder.DropTable(
                name: "difference_severities",
                schema: "checker");

            migrationBuilder.DropTable(
                name: "check_run_statuses",
                schema: "checker");

            migrationBuilder.DropTable(
                name: "spec_snapshots",
                schema: "checker");

            migrationBuilder.DropTable(
                name: "spec_contents",
                schema: "checker");

            migrationBuilder.DropTable(
                name: "spec_documents",
                schema: "checker");

            migrationBuilder.DropTable(
                name: "spec_formats",
                schema: "checker");

            migrationBuilder.DropTable(
                name: "spec_sources",
                schema: "checker");
        }
    }
}
