using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge.Api;

// islevi: Bir snapshot'in tamamen tuketilmis operasyon envanterini tasir.
// sistemdeki gorevi: Grounding ve coverage tuketicilerine sayfa ayrintisi yerine eksiksizlik kaniti sunar.
public sealed class SnapshotOperationInventory
{
    public string OutcomeCode { get; set; } = string.Empty;
    public long TotalCount { get; set; }
    public bool IsComplete { get; set; }
    public List<SnapshotOperation> Items { get; set; } = [];
}
