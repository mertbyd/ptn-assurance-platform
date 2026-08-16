using System;
using System.Collections.Generic;

namespace Ptn.TestModule.Models.Catalog;

// islevi: Tek bir API snapshot'ina muhurlu senaryolarin dokundugu operasyon kumesini tasir.
// sistemdeki gorevi: Kapsamin snapshot bazindaki payini ve kanitli toplam operasyon sayisini tasir.
public class ScenarioCoverageSnapshotGroup
{
    public Guid? SpecSnapshotId { get; set; }
    public int ScenarioCount { get; set; }
    public IReadOnlyList<string> TouchedOperations { get; set; } = [];

    /// <summary>Snapshot'taki toplam operasyon sayisidir; checker kaniti yoksa null'dur.</summary>
    public int? TotalOperationCount { get; set; }
}
