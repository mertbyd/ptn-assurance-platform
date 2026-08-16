using System.Collections.Generic;
using System.Linq;
using Ptn.DatabaseChecker.Constants.Diagnosis;
using Ptn.DatabaseChecker.Interface.Diagnosis;
using Ptn.DatabaseChecker.Models.Diagnosis;
using Volo.Abp.DependencyInjection;

namespace Ptn.DatabaseChecker.Managers.Diagnosis.Rules;

// islevi: Assertion'in bekledigi kolonlardan en az birinin canli tablo katalogunda bulunmadigi hipotezini degerlendirir.
// sistemdeki gorevi: Kolon eksigini provider sorgusu eklemeden snapshot olgusuyla Confirmed kanita cevirir.
[ExposeServices(typeof(IDiagnosisRule))]
public sealed class ExpectedColumnMissingRule : IDiagnosisRule, ITransientDependency
{
    public string HypothesisKindCode => HypothesisKindCodes.ExpectedColumnMissing;
    public int Priority => 95;

    // islevi: Canli katalog ile failed-expectation kolonlari arasinda eksik ad varsa adayi acar.
    public bool AppliesTo(FailureIdentity identity, ResolvedFailureContext context)
        => identity.IndicatesMissingColumn || context.MissingExpectedColumns.Count > 0;

    // islevi: Snapshot kolon olgusu yeterli oldugu icin ek probe istemez.
    public List<ProbeRequest> RequiredProbes(FailureIdentity identity, ResolvedFailureContext context)
        => new();

    // islevi: Ilk uc eksik kolon olgusunu veri degeri tasimadan Confirmed katalog kanitina cevirir.
    public HypothesisAssessment Assess(
        FailureIdentity identity,
        ResolvedFailureContext context,
        List<ProbeEvidence> evidence)
    {
        var proofs = context.MissingExpectedColumns.Take(3)
            .Select(_ => ProbeEvidence.Catalog(HypothesisKindCode))
            .ToList();
        if (proofs.Count == 0)
        {
            proofs.Add(ProbeEvidence.Catalog(HypothesisKindCode));
        }

        return new HypothesisAssessment(
            HypothesisKindCode,
            Priority,
            DiagnosisConfidenceCodes.Confirmed,
            proofs);
    }
}
