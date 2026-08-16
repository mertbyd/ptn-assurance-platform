using System.Collections.Generic;
using Ptn.DatabaseChecker.Constants.Diagnosis;
using Ptn.DatabaseChecker.Interface.Diagnosis;
using Ptn.DatabaseChecker.Models.Diagnosis;
using Volo.Abp.DependencyInjection;

namespace Ptn.DatabaseChecker.Managers.Diagnosis.Rules;

// islevi: Timeout sonrasinda tam assertion anahtariyla satirin sonradan olustugu hipotezini degerlendirir.
// sistemdeki gorevi: KBP-704 TimedOut olgusunu yeni RowExists kanitiyla gecikmeli yazma sonucuna cevirir.
[ExposeServices(typeof(IDiagnosisRule))]
public sealed class RowCreatedLateRule : IDiagnosisRule, ITransientDependency
{
    public string HypothesisKindCode => HypothesisKindCodes.RowCreatedLate;
    public int Priority => 80;

    // islevi: Yalniz timeout ile bitmis ve tam anahtari katalogda sorgulanabilir satiri aday yapar.
    public bool AppliesTo(FailureIdentity identity, ResolvedFailureContext context)
        => context.RowTimedOut && context.TargetStructure is not null && context.TargetKeyValues.Count > 0;

    // islevi: Timeout sonrasinda ayni tam assertion anahtariyla satir varligini yeniden yoklar.
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
                KeyValues = context.TargetKeyValues
            }
        };

    // islevi: Satir simdi varsa gecikmeli yaratimi kanitlar, hala yoksa curutur, eksik probe'da Possible birakir.
    public HypothesisAssessment Assess(
        FailureIdentity identity,
        ResolvedFailureContext context,
        List<ProbeEvidence> evidence)
    {
        var proof = ProbeEvidence.Find(evidence, HypothesisKindCode, ProbeKindCodes.RowExists);
        var confidence = proof?.FactCode switch
        {
            ProbeKindCodes.Facts.Found => DiagnosisConfidenceCodes.Confirmed,
            ProbeKindCodes.Facts.Missing => DiagnosisConfidenceCodes.RuledOut,
            _ => DiagnosisConfidenceCodes.Possible
        };
        return new HypothesisAssessment(HypothesisKindCode, Priority, confidence, proof is null ? null : new() { proof });
    }
}
