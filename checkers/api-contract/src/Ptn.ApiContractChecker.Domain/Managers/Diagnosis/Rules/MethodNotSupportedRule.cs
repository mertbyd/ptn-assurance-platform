using Ptn.ApiContractChecker.Constants.Diagnosis;
using Ptn.ApiContractChecker.Models.Diagnosis;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Diagnosis.Rules;

// islevi: Ortam Allow metotlari ile gonderilen metodu karsilastirip desteklenmiyor hipotezini degerlendirir.
// sistemdeki gorevi: H-EN-02'yi fixed OPTIONS probe'u ve snapshot path allow-list'iyle saglar.
public sealed class MethodNotSupportedRule : DiagnosisRuleBase, ITransientDependency
{
    public override string HypothesisKindCode => HypothesisKindCodes.MethodNotSupported;
    public override int Priority => 66;
    public override bool AppliesTo(FailureIdentity identity, ResolvedFailureContext context)
        => context.Operation != null && context.Snapshot.Servers.Count > 0;

    // islevi: Server ve sinyal path'inden snapshot guardli OPTIONS hedefi kurar.
    public override List<ProbeRequest> RequiredProbes(FailureIdentity identity, ResolvedFailureContext context)
    {
        var server = new Uri(context.Snapshot.Servers.OrderBy(item => item, StringComparer.Ordinal).First());
        return [CreateProbe(ProbeKindCodes.OptionsAllow, context,
            targetUri: new Uri(server, context.Signal.Path ?? context.Operation!.Path))];
    }

    // islevi: Allow kaniti gonderilen metodu icermiyorsa Confirmed, iceriyorsa RuledOut sonucunu verir.
    public override HypothesisAssessment Assess(
        FailureIdentity identity,
        ResolvedFailureContext context,
        List<ProbeEvidence> evidence)
    {
        var proof = FindEvidence(evidence, ProbeKindCodes.OptionsAllow);
        var methods = (proof?.ObservedValue ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries);
        var supported = methods.Contains(context.Signal.Method, StringComparer.OrdinalIgnoreCase);
        var confidence = proof is null ? DiagnosisConfidenceCodes.Possible : supported
            ? DiagnosisConfidenceCodes.RuledOut
            : DiagnosisConfidenceCodes.Confirmed;
        return CreateAssessment(confidence, proof is null ? [] : [proof]);
    }
}
