using System.Collections.Generic;
using System.Linq;
using Ptn.DatabaseChecker.Constants.Diagnosis;
using Ptn.DatabaseChecker.Interface.Diagnosis;
using Ptn.DatabaseChecker.Models.Diagnosis;
using Volo.Abp.DependencyInjection;

namespace Ptn.DatabaseChecker.Managers.Diagnosis.Rules;

// islevi: Satir mevcutken kolon matcher beklentilerinin gozlenen degerlerden farkli oldugu hipotezini degerlendirir.
// sistemdeki gorevi: KBP-704 failedExpectations kanitini yeni hedef sorgusu yapmadan Confirmed teshise tasir.
[ExposeServices(typeof(IDiagnosisRule))]
public sealed class RowValueDiffersRule : IDiagnosisRule, ITransientDependency
{
    public string HypothesisKindCode => HypothesisKindCodes.RowValueDiffers;
    public int Priority => 100;

    // islevi: Yalniz value-mismatch olgusu ve en az bir redaction uygulanmis failure kaniti varsa adayi acar.
    public bool AppliesTo(FailureIdentity identity, ResolvedFailureContext context)
        => context.ValueWasReportedDifferent && context.FailedExpectations.Count > 0;

    // islevi: Assertion failure kaniti yeterli oldugu icin ek hedef probe'u istemez.
    public List<ProbeRequest> RequiredProbes(FailureIdentity identity, ResolvedFailureContext context)
        => new();

    // islevi: Ilk uc failed-expectation degerini katalog kaniti olarak tasiyip hipotezi Confirmed yapar.
    public HypothesisAssessment Assess(
        FailureIdentity identity,
        ResolvedFailureContext context,
        List<ProbeEvidence> evidence)
    {
        var proofs = context.FailedExpectations.Take(3)
            .Select(item => ProbeEvidence.Catalog(
                HypothesisKindCode,
                item.ExpectedValue,
                item.ObservedValue))
            .ToList();
        return new HypothesisAssessment(
            HypothesisKindCode,
            Priority,
            DiagnosisConfidenceCodes.Confirmed,
            proofs);
    }
}
