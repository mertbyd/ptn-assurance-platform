using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ptn.TestModule.Migrations
{
    /// <inheritdoc />
    public partial class ScenarioHealthView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Saglik tablo degil view olarak baslar (PLAN-0003 TM-27). Pass/fail/flaky oranlari ve p95
            // burada hesaplanir; uygulama tarafinda satir toplayip bellekte yuzdelik hesaplanmaz.
            // is_dry_run = true satirlar hesaba girmez (TM-18).
            // TenantId NULL tasiyan host satirlari bos uuid'e indirgenir; boylece CONCURRENTLY yenilemenin
            // zorunlu kildigi benzersiz indeks NULL tasimaz. ABP taban kolonlari snake_case'e cevrilmez,
            // bu yuzden yalniz "TenantId" tirnakli ve PascalCase adreslenir (mevcut migration'larla ayni).
            migrationBuilder.Sql(
                """
                CREATE MATERIALIZED VIEW test_run.scenario_health AS
                WITH terminal_run AS (
                    SELECT
                        COALESCE(r."TenantId", '00000000-0000-0000-0000-000000000000'::uuid) AS tenant_key,
                        r.test_key,
                        r.history_id,
                        r.completed_at,
                        o.code AS outcome_code,
                        latest.duration_ms
                    FROM test_run.test_runs r
                    JOIN LATERAL (
                        SELECT res.outcome_status_id, res.duration_ms
                        FROM test_run.test_run_results res
                        WHERE res.test_run_id = r.id
                        ORDER BY res.attempt DESC
                        LIMIT 1
                    ) latest ON TRUE
                    JOIN test_lookup.test_outcome_statuses o ON o.id = latest.outcome_status_id
                    WHERE r.is_dry_run = false
                ),
                history_bucket AS (
                    SELECT
                        tenant_key,
                        test_key,
                        history_id,
                        count(*) FILTER (WHERE outcome_code = 'Passed') AS passed_count,
                        count(*) FILTER (WHERE outcome_code <> 'Passed') AS failed_count
                    FROM terminal_run
                    WHERE history_id IS NOT NULL
                    GROUP BY tenant_key, test_key, history_id
                ),
                bucket_summary AS (
                    SELECT
                        tenant_key,
                        test_key,
                        count(*) AS history_count,
                        count(*) FILTER (WHERE passed_count > 0 AND failed_count > 0) AS flaky_history_count
                    FROM history_bucket
                    GROUP BY tenant_key, test_key
                )
                SELECT
                    t.tenant_key                                                          AS tenant_key,
                    t.test_key                                                            AS scenario_key,
                    count(*)                                                              AS total_run_count,
                    count(*) FILTER (WHERE t.outcome_code = 'Passed')                     AS passed_run_count,
                    count(*) FILTER (WHERE t.outcome_code <> 'Passed')                    AS failed_run_count,
                    COALESCE(b.history_count, 0)                                          AS history_count,
                    COALESCE(b.flaky_history_count, 0)                                    AS flaky_history_count,
                    (count(*) FILTER (WHERE t.outcome_code = 'Passed'))::double precision
                        / count(*)                                                        AS pass_ratio,
                    COALESCE(
                        b.flaky_history_count::double precision / NULLIF(b.history_count, 0),
                        0)                                                                AS flaky_ratio,
                    percentile_cont(0.95) WITHIN GROUP (
                        ORDER BY t.duration_ms::double precision)                         AS p95_duration_ms,
                    max(t.completed_at)                                                   AS last_run_at
                FROM terminal_run t
                LEFT JOIN bucket_summary b
                    ON b.tenant_key = t.tenant_key AND b.test_key = t.test_key
                GROUP BY t.tenant_key, t.test_key, b.history_count, b.flaky_history_count;
                """);

            // REFRESH MATERIALIZED VIEW CONCURRENTLY benzersiz indeks ister; indeks olmadan yenileme calismaz.
            migrationBuilder.Sql(
                """
                CREATE UNIQUE INDEX ux_scenario_health
                ON test_run.scenario_health (tenant_key, scenario_key);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Indeks view'a bagli oldugu icin view birlikte dusurulur; tablo veya kolon silinmez.
            migrationBuilder.Sql("DROP INDEX IF EXISTS test_run.ux_scenario_health;");
            migrationBuilder.Sql("DROP MATERIALIZED VIEW IF EXISTS test_run.scenario_health;");
        }
    }
}
