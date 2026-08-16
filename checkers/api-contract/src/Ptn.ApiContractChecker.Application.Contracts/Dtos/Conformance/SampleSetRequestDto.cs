using Ptn.ApiContractChecker.Constants.Conformance;
using Ptn.ApiContractChecker.Constants.Conformance.Lookups;

namespace Ptn.ApiContractChecker.Dtos.Conformance;

// islevi: Snapshot operasyonu ve ornek uretim butcesini public request govdesinde tasir.
// sistemdeki gorevi: OperationId veya method/path secimini kapali sample turu ile HTTP sinirinda birlestirir.
public class SampleSetRequestDto
{
    public Guid SnapshotId { get; set; }
    public string? OperationId { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string SampleKindCode { get; set; } = SampleKindCodes.Both;
    public int MaxSamplesPerField { get; set; } = SampleGenerationConsts.MaxSamplesPerField;
}
