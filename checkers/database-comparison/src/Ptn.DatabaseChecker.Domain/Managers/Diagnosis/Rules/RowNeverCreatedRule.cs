using System.Collections.Generic;
using Ptn.DatabaseChecker.Constants.Diagnosis;
using Ptn.DatabaseChecker.Interface.Diagnosis;
using Ptn.DatabaseChecker.Models.Diagnosis;
using Volo.Abp.DependencyInjection;

namespace Ptn.DatabaseChecker.Managers.Diagnosis.Rules;

// islevi: Beklenen satirin unique kimlik anahtariyla canli hedefte hic bulunmadigi hipotezini degerlendirir.
// sistemdeki gorevi: Missing-row olgusunu sinirli RowExists kanitiyla Confirmed veya RuledOut sonucuna cevirir.
[ExposeServices(typeof(IDiagnosisRule))]
public sealed class RowNeverCreatedRule : IDiagnosisRule, ITransientDependency
{
    public string HypothesisKindCode => HypothesisKindCodes.RowNeverCreated;
    public int Priority => 70;

    // islevi: Yalniz missing-row olgusu ve katalogda dogrulanmis kimlik anahtari varsa adayi acar.
    public bool AppliesTo(FailureIdentity identity, ResolvedFailureContext context)
        => context.RowWasReportedMissing && context.TargetStructure is not null && context.IdentityKeyValues.Count > 0;

    // islevi: En dar dogrulanmis unique kimlikle hedef satir varligini yoklar.
    public List<ProbeRequest> RequiredProbes(FailureIdentity identity, ResolvedFailureContext context)
        => new()
        {
            new ProbeRequest
            {
                ProbeKindCode = ProbeKindCodes.RowExists,
                HypothesisKindCode = HypothesisKindCode,
                SchemaName = context.TargetStructure!.SchemaName,
                TableName = context.TargetStructure.TableName,
                Structure = context.TargetStructure,
                KeyValues = context.IdentityKeyValues
            }
        };

    // islevi: Satir yoksa hipotezi kanitlar, varsa curutur; probe calismadiysa Possible birakir.
    public HypothesisAssessment Assess(
        FailureIdentity identity,
        ResolvedFailureContext context,
        List<ProbeEvidence> evidence)
    {
        var proof = ProbeEvidence.Find(evidence, HypothesisKindCode, ProbeKindCodes.RowExists);
        var confidence = proof?.FactCode switch
        {
            ProbeKindCodes.Facts.Missing => DiagnosisConfidenceCodes.Confirmed,
            ProbeKindCodes.Facts.Found => DiagnosisConfidenceCodes.RuledOut,
            _ => DiagnosisConfidenceCodes.Possible
        };
        return new HypothesisAssessment(HypothesisKindCode, Priority, confidence, proof is null ? null : new() { proof });
    }
}
