using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ptn.TestModule.Migrations
{
    /// <inheritdoc />
    public partial class RunFindingFingerprint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "fingerprint",
                schema: "test_run",
                table: "test_result_findings",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            // Var olan bulgulari TestRunResultManager.CreateFingerprint ile ayni kanonik girdiden geriye doldurur.
            migrationBuilder.Sql(
                """
                UPDATE test_run.test_result_findings
                SET fingerprint = encode(
                    sha256(convert_to(
                        source_checker_code || '|' || comparison_kind_code || '|' ||
                        coalesce(rule_ref, '') || '|' || location,
                        'UTF8')),
                    'hex')
                WHERE fingerprint = '';
                """);

            migrationBuilder.CreateIndex(
                name: "ix_findings_fingerprint",
                schema: "test_run",
                table: "test_result_findings",
                column: "fingerprint");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_findings_fingerprint",
                schema: "test_run",
                table: "test_result_findings");

            migrationBuilder.DropColumn(
                name: "fingerprint",
                schema: "test_run",
                table: "test_result_findings");
        }
    }
}
