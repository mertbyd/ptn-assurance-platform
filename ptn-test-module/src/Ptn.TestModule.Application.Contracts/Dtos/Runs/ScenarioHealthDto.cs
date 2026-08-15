using System;

namespace Ptn.TestModule.Dtos.Runs;

// islevi: Bir senaryo anahtarinin veritabaninda hesaplanmis saglik ozetini API cevabinda tasir.
// sistemdeki gorevi: Agir rapor, derlenmis belge ve HAR govdesi tasimadan trend gorunumunu sunar (TM-22).
/// <summary>Senaryo saglik ozetinin public gorunumudur.</summary>
public class ScenarioHealthDto
{
    /// <summary>Kosumlarin gruplandigi kararli test anahtaridir.</summary>
    public string ScenarioKey { get; set; } = string.Empty;

    /// <summary>Terminal sonucu olan ve dry-run olmayan toplam kosum sayisidir.</summary>
    public long TotalRunCount { get; set; }

    /// <summary>Passed hukmu alan kosum sayisidir.</summary>
    public long PassedRunCount { get; set; }

    /// <summary>Passed disi hukum alan kosum sayisidir.</summary>
    public long FailedRunCount { get; set; }

    /// <summary>Ayni test anahtarindaki farkli trend kovasi sayisidir.</summary>
    public long HistoryCount { get; set; }

    /// <summary>Hem yesil hem kirmizi gormus trend kovasi sayisidir.</summary>
    public long FlakyHistoryCount { get; set; }

    /// <summary>Passed kosumlarin toplam kosuma oranidir.</summary>
    public double PassRatio { get; set; }

    /// <summary>Kararsiz trend kovalarinin tum kovalara oranidir.</summary>
    public double FlakyRatio { get; set; }

    /// <summary>Kosum surelerinin veritabaninda hesaplanan 95. yuzdeligidir.</summary>
    public double P95DurationMs { get; set; }

    /// <summary>Son tamamlanan kosumun zamanidir.</summary>
    public DateTime? LastRunAt { get; set; }
}
