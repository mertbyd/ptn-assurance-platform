using Ptn.ApiContractChecker.Constants.Snapshots.Lookups;

namespace Ptn.ApiContractChecker.Dtos.Conformance;

// islevi: Snapshot icindeki hedef operasyonu tahminsiz secen yazarlik istegini tasir.
// sistemdeki gorevi: Request ornegi ve baglama onerisi endpointlerinin ortak public input DTO'sudur.
public class OperationSelectionDto
{
    public Guid SnapshotId { get; set; }
    public string? OperationId { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string VerbosityCode { get; set; } = SnapshotVerbosityCodes.Minimal;
}
