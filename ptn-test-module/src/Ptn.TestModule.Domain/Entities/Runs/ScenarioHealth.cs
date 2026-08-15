using System;

namespace Ptn.TestModule.Entities.Runs;

// islevi: test_run.scenario_health materialized view satirini salt-okunur veri kabugu olarak tasir.
// sistemdeki gorevi: Pass/fail/flaky oranlari ile p95 suresi veritabaninda hesaplanir; bu tip yalniz sonucu tasir (PLAN-0003 TM-27).
/// <summary>
/// Senaryo saglik materialized view'inin anahtarsiz EF Core satiridir.
/// </summary>
public class ScenarioHealth
{
    /// <summary>Host satirlarini bos Guid'e indirgeyen ve NULL tasimayan tenant anahtaridir.</summary>
    public Guid TenantKey { get; internal set; }

    /// <summary>Kosumlarin gruplandigi kararli test anahtaridir.</summary>
    public string ScenarioKey { get; internal set; } = string.Empty;

    /// <summary>Terminal sonucu olan ve dry-run olmayan toplam kosum sayisidir.</summary>
    public long TotalRunCount { get; internal set; }

    /// <summary>Passed hukmu alan kosum sayisidir.</summary>
    public long PassedRunCount { get; internal set; }

    /// <summary>Passed disi hukum alan kosum sayisidir.</summary>
    public long FailedRunCount { get; internal set; }

    /// <summary>Ayni test anahtarindaki farkli trend kovasi sayisidir.</summary>
    public long HistoryCount { get; internal set; }

    /// <summary>Ayni trend kovasi icinde hem yesil hem kirmizi gorulen kova sayisidir.</summary>
    public long FlakyHistoryCount { get; internal set; }

    /// <summary>Passed kosumlarin toplam kosuma oranidir.</summary>
    public double PassRatio { get; internal set; }

    /// <summary>Kararsiz trend kovalarinin tum kovalara oranidir.</summary>
    public double FlakyRatio { get; internal set; }

    /// <summary>Kosum surelerinin percentile_cont ile hesaplanan 95. yuzdeligidir.</summary>
    public double P95DurationMs { get; internal set; }

    /// <summary>Son tamamlanan kosumun zamanidir.</summary>
    public DateTime? LastRunAt { get; internal set; }
}
