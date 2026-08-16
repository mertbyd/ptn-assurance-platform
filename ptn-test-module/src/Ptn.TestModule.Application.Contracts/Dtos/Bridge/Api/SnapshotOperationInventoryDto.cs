namespace Ptn.TestModule.Dtos.Bridge.Api;

// islevi: Tam snapshot operasyon envanterini sonuc ve eksiksizlik bilgisiyle tasir.
// sistemdeki gorevi: Grounding ve coverage akisini checker sayfalama sozlesmesinden yalitir.
public sealed class SnapshotOperationInventoryDto
{
    public string OutcomeCode { get; set; } = string.Empty;
    public long TotalCount { get; set; }
    public bool IsComplete { get; set; }
    public List<SnapshotOperationDto> Items { get; set; } = [];
}
