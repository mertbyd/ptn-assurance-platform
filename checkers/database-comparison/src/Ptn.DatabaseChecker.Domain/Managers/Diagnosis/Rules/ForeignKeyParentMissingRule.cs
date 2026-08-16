using System.Collections.Generic;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Constants.Diagnosis;
using Ptn.DatabaseChecker.Interface.Diagnosis;
using Ptn.DatabaseChecker.Models.Diagnosis;
using Volo.Abp.DependencyInjection;

namespace Ptn.DatabaseChecker.Managers.Diagnosis.Rules;

// islevi: Dogrulanmis FK'nin referenced anahtar degerleriyle parent satirin bulunmadigi hipotezini degerlendirir.
// sistemdeki gorevi: FK constraint olgusunu parent tablo RowExists kanitiyla kod esitliginden bagimsiz teshis eder.
[ExposeServices(typeof(IDiagnosisRule))]
public sealed class ForeignKeyParentMissingRule : IDiagnosisRule, ITransientDependency
{
    public string HypothesisKindCode => HypothesisKindCodes.ForeignKeyParentMissing;
    public int Priority => 98;

    // islevi: FK turu, cozulmus parent yapisi ve eksiksiz referenced anahtar olgularini arar.
    public bool AppliesTo(FailureIdentity identity, ResolvedFailureContext context)
        => context.Constraint?.TypeCode == SchemaConstraintTypeCodes.ForeignKey &&
           context.ParentStructure is not null &&
           context.ParentKeyValues.Count > 0;

    // islevi: FK referenced tablosunda dogrulanmis parent anahtariyla satir varligini yoklar.
    public List<ProbeRequest> RequiredProbes(FailureIdentity identity, ResolvedFailureContext context)
        => new()
        {
            new ProbeRequest
            {
                ProbeKindCode = ProbeKindCodes.RowExists,
                HypothesisKindCode = HypothesisKindCode,
                SchemaName = context.ParentStructure!.SchemaName,
                TableName = context.ParentStructure.TableName,
                Structure = context.ParentStructure,
                KeyValues = context.ParentKeyValues
            }
        };

    // islevi: Parent yoksa hipotezi kanitlar, varsa curutur, probe butce disi kalirsa Possible dondurur.
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
