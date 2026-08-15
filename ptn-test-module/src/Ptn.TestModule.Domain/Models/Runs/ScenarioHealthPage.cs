using System.Collections.Generic;
using Ptn.TestModule.Entities.Runs;

namespace Ptn.TestModule.Models.Runs;

// islevi: Senaryo saglik sayfasinin toplam sayisini ve satirlarini birlikte tasir.
// sistemdeki gorevi: TestRunHeaderPage ile ayni sayfa sozlesmesini saglik yuzeyine tasir.
public class ScenarioHealthPage
{
    public long TotalCount { get; set; }
    public IReadOnlyList<ScenarioHealth> Items { get; set; } = [];
}
