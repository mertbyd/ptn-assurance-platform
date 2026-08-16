using System.Collections.Generic;
using Ptn.DatabaseChecker.Constants.Diagnosis;
using Ptn.DatabaseChecker.Interface.Diagnosis;
using Ptn.DatabaseChecker.Models.Diagnosis;
using Volo.Abp.DependencyInjection;

namespace Ptn.DatabaseChecker.Managers.Diagnosis.Rules;

// islevi: Canli katalogdaki constraint'in NOT VALID veya dogrulanmamis olmasi hipotezini degerlendirir.
// sistemdeki gorevi: KBP-703 IsValidated olgusunu yeni sorgu yapmadan Likely veya RuledOut sonucuna cevirir.
[ExposeServices(typeof(IDiagnosisRule))]
public sealed class ConstraintNotValidatedRule : IDiagnosisRule, ITransientDependency
{
    public string HypothesisKindCode => HypothesisKindCodes.ConstraintNotValidated;
    public int Priority => 88;

    // islevi: Yalniz canli katalogda ad/tur olarak cozulmus constraint varsa adayi acar.
    public bool AppliesTo(FailureIdentity identity, ResolvedFailureContext context)
        => context.Constraint is not null;

    // islevi: IsValidated katalog olgusu yeterli oldugu icin ek probe istemez.
    public List<ProbeRequest> RequiredProbes(FailureIdentity identity, ResolvedFailureContext context)
        => new();

    // islevi: Dogrulanmamis constraint'i Likely, dogrulanmis constraint'i RuledOut katalog kanitina cevirir.
    public HypothesisAssessment Assess(
        FailureIdentity identity,
        ResolvedFailureContext context,
        List<ProbeEvidence> evidence)
        => new(
            HypothesisKindCode,
            Priority,
            context.Constraint!.IsValidated
                ? DiagnosisConfidenceCodes.RuledOut
                : DiagnosisConfidenceCodes.Likely,
            new() { ProbeEvidence.Catalog(HypothesisKindCode) });
}
