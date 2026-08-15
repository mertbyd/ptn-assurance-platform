using System;
using System.Collections.Generic;

namespace Ptn.TestModule.Models.Catalog;

// islevi: Tek bir API snapshot'ina muhurlu senaryolarin dokundugu operasyon kumesini tasir.
// sistemdeki gorevi: Kapsamin snapshot bazindaki payidir; toplam operasyon sayisi bu tarafta bilinmez.
public class ScenarioCoverageSnapshotGroup
{
    public Guid? SpecSnapshotId { get; set; }
    public int ScenarioCount { get; set; }
    public IReadOnlyList<string> TouchedOperations { get; set; } = [];

    /// <summary>Snapshot'taki toplam operasyon sayisi checker'da acilmadigi icin daima null'dur.</summary>
    public int? TotalOperationCount { get; set; }
}
