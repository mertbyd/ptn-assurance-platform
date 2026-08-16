using System.Collections.Generic;
using Ptn.DatabaseChecker.Constants.Diagnosis;
using Ptn.DatabaseChecker.Interface.Diagnosis;
using Ptn.DatabaseChecker.Models.Diagnosis;
using Volo.Abp.DependencyInjection;

namespace Ptn.DatabaseChecker.Managers.Diagnosis.Rules;

// islevi: Tam tenant/filter anahtari bulunmazken daha dar unique kimlikle satirin baska scope'ta var oldugu hipotezini degerlendirir.
// sistemdeki gorevi: Serbest WHERE olmadan katalog unique anahtari uzerinden KeyMatchCount kaniti uretir.
[ExposeServices(typeof(IDiagnosisRule))]
public sealed class RowInAnotherScopeRule : IDiagnosisRule, ITransientDependency
{
    public string HypothesisKindCode => HypothesisKindCodes.RowInAnotherScope;
    public int Priority => 90;

    // islevi: Missing-row ve tam anahtardan daha dar dogrulanmis unique kimlik olgularini birlikte arar.
    public bool AppliesTo(FailureIdentity identity, ResolvedFailureContext context)
        => context.RowWasReportedMissing && context.TargetStructure is not null && context.HasBroaderIdentityKey();

    // islevi: Daha dar katalog unique kimligiyle kesin eslesme sayisini ister.
    public List<ProbeRequest> RequiredProbes(FailureIdentity identity, ResolvedFailureContext context)
        => new()
        {
            new ProbeRequest
            {
                ProbeKindCode = ProbeKindCodes.KeyMatchCount,
                HypothesisKindCode = HypothesisKindCode,
                SchemaName = context.TargetStructure!.SchemaName,
                TableName = context.TargetStructure.TableName,
                Structure = context.TargetStructure,
                KeyValues = context.IdentityKeyValues
            }
        };

    // islevi: Dar kimlik eslesmesi varsa farkli scope'u kanitlar, yoksa curutur, eksik probe'da Possible birakir.
    public HypothesisAssessment Assess(
        FailureIdentity identity,
        ResolvedFailureContext context,
        List<ProbeEvidence> evidence)
    {
        var proof = ProbeEvidence.Find(evidence, HypothesisKindCode, ProbeKindCodes.KeyMatchCount);
        var confidence = proof?.FactCode switch
        {
            ProbeKindCodes.Facts.Found => DiagnosisConfidenceCodes.Confirmed,
            ProbeKindCodes.Facts.Missing => DiagnosisConfidenceCodes.RuledOut,
            _ => DiagnosisConfidenceCodes.Possible
        };
        return new HypothesisAssessment(HypothesisKindCode, Priority, confidence, proof is null ? null : new() { proof });
    }
}
