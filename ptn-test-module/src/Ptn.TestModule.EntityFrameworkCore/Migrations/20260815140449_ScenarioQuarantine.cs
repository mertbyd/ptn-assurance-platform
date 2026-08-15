using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ptn.TestModule.Migrations
{
    /// <inheritdoc />
    public partial class ScenarioQuarantine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "quarantine_reason",
                schema: "test_catalog",
                table: "test_scenarios",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "quarantine_until",
                schema: "test_catalog",
                table: "test_scenarios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_scenarios_quarantine",
                schema: "test_catalog",
                table: "test_scenarios",
                column: "quarantine_until");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_scenarios_quarantine",
                schema: "test_catalog",
                table: "test_scenarios");

            migrationBuilder.DropColumn(
                name: "quarantine_reason",
                schema: "test_catalog",
                table: "test_scenarios");

            migrationBuilder.DropColumn(
                name: "quarantine_until",
                schema: "test_catalog",
                table: "test_scenarios");
        }
    }
}
