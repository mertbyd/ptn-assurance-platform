using System.Collections.Generic;

namespace Ptn.TestModule.Models.Catalog;

// islevi: Yayinlanmis senaryolarin dokundugu operasyon ve kural kumelerini snapshot bazinda tasir.
// sistemdeki gorevi: Kapsamin yalniz payini bildirir; payda checker'da acilmadigi icin bilinmiyor isaretlenir.
public class ScenarioCoverageReport
{
    public int PublishedScenarioCount { get; set; }
    public IReadOnlyList<ScenarioCoverageSnapshotGroup> Snapshots { get; set; } = [];
    public IReadOnlyList<ScenarioCoverageRuleGroup> Rules { get; set; } = [];

    /// <summary>Paydanin neden bilinmedigini bildiren kararli gerekce kodudur.</summary>
    public string DenominatorState { get; set; } = string.Empty;
    public string DenominatorUnknownReason { get; set; } = string.Empty;
}
