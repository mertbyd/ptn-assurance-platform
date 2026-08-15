using Ptn.TestModule.Constants.Bridge;

namespace Ptn.TestModule.Dtos.Bridge.Api;

// islevi: Operasyon zinciri onerisi icin snapshot, kaynak operasyon ve aday tavanini tasir.
// sistemdeki gorevi: ACX-07 yazarlik yuzeyini public Bridge sozlesmesinde tipli tutar.
public sealed class OperationLinkRequestDto
{
    public Guid SnapshotId { get; set; }
    public string SourceOperationId { get; set; } = string.Empty;
    public int MaxCandidates { get; set; } = PtnBridgeConsts.DefaultOperationLinkCandidates;
}
