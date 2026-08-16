using Ptn.ApiContractChecker.Constants.Diagnosis;
using Ptn.ApiContractChecker.Models.Diagnosis;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Diagnosis.Rules;

// islevi: Snapshot'ta bulunan operasyonun erisilebilir serverda deploy edilmemis olma hipotezini degerlendirir.
// sistemdeki gorevi: H-EN-01'i tekil status eslemesi yerine operasyon ve server reachability olgularina baglar.
public sealed class PathNotDeployedRule : DiagnosisRuleBase, ITransientDependency
{
    public override string HypothesisKindCode => HypothesisKindCodes.PathNotDeployed;
    public override int Priority => 65;
    public override bool AppliesTo(FailureIdentity identity, ResolvedFailureContext context)
        => context.Operation != null && identity.StatusClassCode == HttpStatusClassCodes.ClientError &&
           context.Snapshot.Servers.Count > 0;

    // islevi: Ilk snapshot server kokunu fixed GET reachability probe'una verir.
    public override List<ProbeRequest> RequiredProbes(FailureIdentity identity, ResolvedFailureContext context)
        => [CreateProbe(ProbeKindCodes.ServerReachability, context,
            targetUri: new Uri(context.Snapshot.Servers.OrderBy(item => item, StringComparer.Ordinal).First()))];

    // islevi: Server ulasilabilirken operation client error veriyorsa Likely, ulasilamiyorsa RuledOut sonucunu verir.
    public override HypothesisAssessment Assess(
        FailureIdentity identity,
        ResolvedFailureContext context,
        List<ProbeEvidence> evidence)
    {
        var proof = FindEvidence(evidence, ProbeKindCodes.ServerReachability);
        var confidence = proof?.FactCode == ProbeKindCodes.Facts.Reachable
            ? DiagnosisConfidenceCodes.Likely
            : proof is null ? DiagnosisConfidenceCodes.Possible : DiagnosisConfidenceCodes.RuledOut;
        return CreateAssessment(confidence, proof is null ? [] : [proof]);
    }
}
