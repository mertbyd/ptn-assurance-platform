using Ptn.ApiContractChecker.Constants.Diagnosis;
using Ptn.ApiContractChecker.Models.Diagnosis;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Diagnosis.Rules;

// islevi: Referans verilen kaynagin HEAD ile hic gorunmedigi hipotezini degerlendirir.
// sistemdeki gorevi: H-ST-01'i safe network kaniti ve tipli database sonraki kontroluyle saglar.
public sealed class ResourceNeverCreatedRule : DiagnosisRuleBase, ITransientDependency
{
    public override string HypothesisKindCode => HypothesisKindCodes.ResourceNeverCreated;
    public override int Priority => 85;

    // islevi: Yapilandirilmis ve mutlak resource URL varsa hipotezi aday yapar.
    public override bool AppliesTo(FailureIdentity identity, ResolvedFailureContext context)
        => Uri.TryCreate(context.Signal.ResourceUrl, UriKind.Absolute, out _);

    // islevi: Kaynak hedefi icin fixed HEAD probe istegi uretir.
    public override List<ProbeRequest> RequiredProbes(FailureIdentity identity, ResolvedFailureContext context)
        => [CreateProbe(ProbeKindCodes.HeadResource, context, targetUri: new Uri(context.Signal.ResourceUrl!))];

    // islevi: HEAD absent ise Confirmed, present ise RuledOut, kanit yoksa Possible sonucunu verir.
    public override HypothesisAssessment Assess(
        FailureIdentity identity,
        ResolvedFailureContext context,
        List<ProbeEvidence> evidence)
    {
        var proof = FindEvidence(evidence, ProbeKindCodes.HeadResource);
        var confidence = proof?.FactCode switch
        {
            ProbeKindCodes.Facts.Absent => DiagnosisConfidenceCodes.Confirmed,
            ProbeKindCodes.Facts.Present => DiagnosisConfidenceCodes.RuledOut,
            _ => DiagnosisConfidenceCodes.Possible
        };
        return CreateAssessment(confidence, proof is null ? [] : [proof], BuildSuggestedCheck(confidence, context));
    }

    // islevi: Kesin kayip kaynakta composition hostun cozecegi database row kontrolunu kurar.
    private static SuggestedCheck? BuildSuggestedCheck(string confidence, ResolvedFailureContext context)
        => confidence == DiagnosisConfidenceCodes.Confirmed
            ? new SuggestedCheck
            {
                CapabilityCode = SuggestedCheckCodes.DatabaseCapability,
                OperationCode = SuggestedCheckCodes.AssertRowOperation,
                Arguments = new Dictionary<string, string?>
                {
                    [SuggestedCheckCodes.OperationIdArgument] = context.Operation?.OperationId,
                    [SuggestedCheckCodes.ResourceUrlArgument] = context.Signal.ResourceUrl
                }
            }
            : null;
}
