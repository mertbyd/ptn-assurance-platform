using System;
using System.Collections.Generic;

namespace Ptn.TestModule.Dtos.Catalog;

// islevi: Tek API snapshot'ina muhurlu senaryolarin dokundugu operasyon kumesini API cevabinda tasir.
// sistemdeki gorevi: Payi acikca gosterir; toplam operasyon sayisi checker'da olmadigi icin null kalir.
/// <summary>Snapshot bazinda kapsam payinin public gorunumudur.</summary>
public class ScenarioCoverageSnapshotDto
{
    /// <summary>Senaryolarin muhurlendigi API Checker snapshot kimligidir.</summary>
    public Guid? SpecSnapshotId { get; set; }

    /// <summary>Bu snapshot'a muhurlu yayinlanmis senaryo sayisidir.</summary>
    public int ScenarioCount { get; set; }

    /// <summary>Derlenmis belgelerden okunan operasyon adresleridir.</summary>
    public List<string> TouchedOperations { get; set; } = [];

    /// <summary>Snapshot'taki toplam operasyon sayisidir; checker ucu acilana kadar daima null'dur.</summary>
    public int? TotalOperationCount { get; set; }
}
