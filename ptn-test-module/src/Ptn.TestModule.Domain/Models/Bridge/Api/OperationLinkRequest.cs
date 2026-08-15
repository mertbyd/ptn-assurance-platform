using System;
using Ptn.TestModule.Constants.Bridge;

namespace Ptn.TestModule.Models.Bridge;

// islevi: Kaynak API operasyonu ile zincir aday butcesini checker cagrisi icin tasir.
// sistemdeki gorevi: Operation link yazarlik girdisini checker DTO'sundan ayirir.
public sealed class OperationLinkRequest
{
    public Guid SnapshotId { get; set; }
    public string SourceOperationId { get; set; } = string.Empty;
    public int MaxCandidates { get; set; } = PtnBridgeConsts.DefaultOperationLinkCandidates;
}
