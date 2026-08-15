using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ptn.TestModule.Migrations
{
    /// <inheritdoc />
    public partial class ScenarioSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "next_run_at",
                schema: "test_catalog",
                table: "test_scenarios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "schedule_cron",
                schema: "test_catalog",
                table: "test_scenarios",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "schedule_enabled",
                schema: "test_catalog",
                table: "test_scenarios",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "ix_scenarios_schedule",
                schema: "test_catalog",
                table: "test_scenarios",
                columns: new[] { "schedule_enabled", "next_run_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_scenarios_schedule",
                schema: "test_catalog",
                table: "test_scenarios");

            migrationBuilder.DropColumn(
                name: "next_run_at",
                schema: "test_catalog",
                table: "test_scenarios");

            migrationBuilder.DropColumn(
                name: "schedule_cron",
                schema: "test_catalog",
                table: "test_scenarios");

            migrationBuilder.DropColumn(
                name: "schedule_enabled",
                schema: "test_catalog",
                table: "test_scenarios");
        }
    }
}
