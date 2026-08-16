using System.Collections.Generic;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Constants.Diagnosis;
using Ptn.DatabaseChecker.Interface.Diagnosis;
using Ptn.DatabaseChecker.Models.Diagnosis;
using Volo.Abp.DependencyInjection;

namespace Ptn.DatabaseChecker.Managers.Diagnosis.Rules;

// islevi: Unique constraint/index anahtar degerleriyle cakisan satirin hedefte zaten var oldugu hipotezini degerlendirir.
// sistemdeki gorevi: Duplicate olgusunu katalog unique yapisi ve sinirli RowExists kanitiyla teshis eder.
[ExposeServices(typeof(IDiagnosisRule))]
public sealed class UniqueDuplicateExistsRule : IDiagnosisRule, ITransientDependency
{
    public string HypothesisKindCode => HypothesisKindCodes.UniqueDuplicateExists;
    public int Priority => 96;

    // islevi: Cikarilmis unique ihlal olgusu ile canli unique constraint/index olgusunu birlikte arar.
    public bool AppliesTo(FailureIdentity identity, ResolvedFailureContext context)
        => identity.IndicatesUniqueViolation &&
           HasUniqueFact(context);

    // islevi: Unique kimlik anahtariyla hedefte cakisan satir varligini yoklar.
    public List<ProbeRequest> RequiredProbes(FailureIdentity identity, ResolvedFailureContext context)
        => CanProbe(context)
            ? new()
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
            }
            : new();

    // islevi: Cakisan satir varsa hipotezi kanitlar, yoksa curutur, anahtar kaniti yoksa Likely birakir.
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
            _ => DiagnosisConfidenceCodes.Likely
        };
        return new HypothesisAssessment(HypothesisKindCode, Priority, confidence, proof is null ? null : new() { proof });
    }

    // islevi: Context'te unique/PK constraint veya filtresiz unique index olgusu bulunup bulunmadigini bildirir.
    private static bool HasUniqueFact(ResolvedFailureContext context)
        => context.UniqueIndex is not null ||
           context.Constraint?.TypeCode is SchemaConstraintTypeCodes.Unique or SchemaConstraintTypeCodes.PrimaryKey;

    // islevi: Unique anahtar degerleri assertion sinyalinden katalog yapisiyla cozulduyse RowExists probe'unun guvenli oldugunu bildirir.
    private static bool CanProbe(ResolvedFailureContext context)
        => context.TargetStructure is not null && context.IdentityKeyValues.Count > 0;
}
