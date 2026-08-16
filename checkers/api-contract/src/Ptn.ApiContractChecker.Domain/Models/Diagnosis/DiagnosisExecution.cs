using Ptn.ApiContractChecker.Interface.Diagnosis;
using Ptn.ApiContractChecker.Models.Runs;
using Ptn.ApiContractChecker.Models.Snapshots;

namespace Ptn.ApiContractChecker.Models.Diagnosis;

// islevi: Yedi adimli teshis akisinin sinyal, snapshot, kimlik, context, rule, kanit ve assessment durumunu tasir.
// sistemdeki gorevi: Ucten fazla iliskili degeri tuple veya callback plumbing'i kullanmadan adimlar arasinda tasir.
public sealed class DiagnosisExecution
{
    public HttpFailureSignal Signal { get; set; } = new();
    public SpecSnapshotModel Snapshot { get; set; } = new();
    public List<Finding> RelatedFindings { get; set; } = new();
    public FailureIdentity Identity { get; set; } = new();
    public ResolvedFailureContext Context { get; set; } = new();
    public List<IDiagnosisRule> Rules { get; set; } = new();
    public List<ProbeEvidence> Evidence { get; set; } = new();
    public List<HypothesisAssessment> Assessments { get; set; } = new();
}
