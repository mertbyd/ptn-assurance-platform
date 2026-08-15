namespace Ptn.TestModule.Constants.Runs;

// islevi: Senaryo saglik materialized view'inin ad, kolon ve indeks sozlesmesini tanimlar.
// sistemdeki gorevi: El yazimi SQL ile EF esleme adlarinin ayni kararli sozlukten okunmasini saglar (RULE-0002).
/// <summary>
/// Senaryo saglik gorunumunun kararli veritabani adlarini ve sorgu sinirlarini tasir.
/// </summary>
public static class ScenarioHealthConsts
{
    /// <summary>Materialized view'in test_run semasindaki adidir.</summary>
    public const string ViewName = "scenario_health";

    /// <summary>CONCURRENTLY yenilemenin zorunlu kildigi benzersiz indeksin adidir.</summary>
    public const string UniqueIndexName = "ux_scenario_health";

    /// <summary>Saglik hesabina giren p95 yuzdelik degeridir.</summary>
    public const string DurationPercentile = "0.95";

    /// <summary>Tek saglik sayfasinda dondurulen varsayilan satir sayisidir.</summary>
    public const int DefaultPageSize = 20;

    /// <summary>Tek saglik sayfasinda dondurulen azami satir sayisidir.</summary>
    public const int MaxPageSize = 200;

    // islevi: View kolon adlarini tek sahipte toplar; view convention ile uretilmedigi icin esleme acikca pinlenir.
    /// <summary>Materialized view kolon adlarini tasir.</summary>
    public static class Columns
    {
        /// <summary>Host satirlarini bos Guid'e indirgeyen ve NULL tasimayan tenant anahtaridir.</summary>
        public const string TenantKey = "tenant_key";

        /// <summary>Kosumun kararli test anahtaridir.</summary>
        public const string ScenarioKey = "scenario_key";

        /// <summary>Terminal sonucu olan toplam kosum sayisidir.</summary>
        public const string TotalRunCount = "total_run_count";

        /// <summary>Passed hukmu alan kosum sayisidir.</summary>
        public const string PassedRunCount = "passed_run_count";

        /// <summary>Passed disi hukum alan kosum sayisidir.</summary>
        public const string FailedRunCount = "failed_run_count";

        /// <summary>Ayni test anahtarindaki farkli trend kovasi sayisidir.</summary>
        public const string HistoryCount = "history_count";

        /// <summary>Hem yesil hem kirmizi gormus trend kovasi sayisidir.</summary>
        public const string FlakyHistoryCount = "flaky_history_count";

        /// <summary>Passed kosumlarin toplam kosuma oranidir.</summary>
        public const string PassRatio = "pass_ratio";

        /// <summary>Kararsiz trend kovalarinin tum kovalara oranidir.</summary>
        public const string FlakyRatio = "flaky_ratio";

        /// <summary>Kosum surelerinin veritabaninda hesaplanan 95. yuzdeligidir.</summary>
        public const string P95DurationMs = "p95_duration_ms";

        /// <summary>Son tamamlanan kosumun zamanidir.</summary>
        public const string LastRunAt = "last_run_at";
    }
}
