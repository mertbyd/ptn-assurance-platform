using Ptn.ApiContractChecker.Constants.Conformance;

namespace Ptn.ApiContractChecker.Dtos.Conformance;

// islevi: Snapshot ve kaynak operationId ile aday tavanini public request govdesinde tasir.
// sistemdeki gorevi: Serbest operasyon tahmini olmadan adim zinciri aday uretimini baslatir.
public class OperationLinkRequestDto
{
    public Guid SnapshotId { get; set; }
    public string SourceOperationId { get; set; } = string.Empty;
    public int MaxCandidates { get; set; } = SampleGenerationConsts.DefaultMaxCandidates;
}
