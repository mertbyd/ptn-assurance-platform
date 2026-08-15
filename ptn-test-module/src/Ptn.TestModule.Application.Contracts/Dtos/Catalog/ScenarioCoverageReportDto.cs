using System.Collections.Generic;

namespace Ptn.TestModule.Dtos.Catalog;

// islevi: Yayinlanmis senaryolarin dokundugu operasyon ve kural kumelerini API cevabinda tasir.
// sistemdeki gorevi: Kapsamin yalniz payini sunar; payda bilinmiyor olarak kodla isaretlenir (KBP-110 §2.2).
/// <summary>Senaryo kapsam raporunun public gorunumudur.</summary>
public class ScenarioCoverageReportDto
{
    /// <summary>Rapora giren yayinlanmis senaryo surumu sayisidir.</summary>
    public int PublishedScenarioCount { get; set; }

    /// <summary>Snapshot bazinda dokunulan operasyon kumeleridir.</summary>
    public List<ScenarioCoverageSnapshotDto> Snapshots { get; set; } = [];

    /// <summary>Bulgu kayitlarindan okunan kural referansi kumeleridir.</summary>
    public List<ScenarioCoverageRuleDto> Rules { get; set; } = [];

    /// <summary>Paydanin durumudur; su an daima bilinmiyordur.</summary>
    public string DenominatorState { get; set; } = string.Empty;

    /// <summary>Paydanin neden bilinmedigini bildiren kararli gerekce kodudur.</summary>
    public string DenominatorUnknownReason { get; set; } = string.Empty;
}
