using Ptn.ApiContractChecker.Models.Runs;
using Ptn.ApiContractChecker.Models.Snapshots;

namespace Ptn.ApiContractChecker.Models.Diagnosis;

// islevi: Hata kimligini snapshot operasyonu ve ilgili karsilastirma bulgulariyla yerellestirir.
// sistemdeki gorevi: Hipotez kurallarina repository veya parser yerine tek provider-bagimsiz olgu baglami verir.
public sealed class ResolvedFailureContext
{
    public SpecSnapshotModel Snapshot { get; set; } = new();
    public FailureIdentity Identity { get; set; } = new();
    public SpecOperationModel? Operation { get; set; }
    public ObjectReference Location { get; set; } = new();
    public List<Finding> RelatedFindings { get; set; } = new();
    public HttpFailureSignal Signal { get; set; } = new();
}
