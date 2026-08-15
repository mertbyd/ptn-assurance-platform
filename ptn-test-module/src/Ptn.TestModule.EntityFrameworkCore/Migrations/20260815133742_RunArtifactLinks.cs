using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ptn.TestModule.Migrations
{
    /// <inheritdoc />
    public partial class RunArtifactLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ctrf_blob_name",
                schema: "test_run",
                table: "test_run_results",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "junit_blob_name",
                schema: "test_run",
                table: "test_run_results",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "sarif_blob_name",
                schema: "test_run",
                table: "test_run_results",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ctrf_blob_name",
                schema: "test_run",
                table: "test_run_results");

            migrationBuilder.DropColumn(
                name: "junit_blob_name",
                schema: "test_run",
                table: "test_run_results");

            migrationBuilder.DropColumn(
                name: "sarif_blob_name",
                schema: "test_run",
                table: "test_run_results");
        }
    }
}
