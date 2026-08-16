using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge.Api;

// islevi: Checker'in tek sayfalik operasyon envanteri wire seklini native sinirda tasir.
// sistemdeki gorevi: Sayfalama metadatasini tam envanter ve opak referans kararindan ayirir.
public sealed class SnapshotOperationPage
{
    public string OutcomeCode { get; set; } = string.Empty;
    public long TotalCount { get; set; }
    public int RequestedMaxResultCount { get; set; }
    public int EffectiveMaxResultCount { get; set; }
    public bool IsTruncated { get; set; }
    public int ResponseBytes { get; set; }
    public List<SnapshotOperationRow> Items { get; set; } = [];
}
