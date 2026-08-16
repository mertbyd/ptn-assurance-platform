using System.Collections.Generic;
using System.Linq;
using Ptn.DatabaseChecker.Constants.Diagnosis;
using Ptn.DatabaseChecker.Interface.Diagnosis;
using Ptn.DatabaseChecker.Models.Diagnosis;
using Volo.Abp.DependencyInjection;

namespace Ptn.DatabaseChecker.Managers.Diagnosis.Rules;

// islevi: search_path veya collation server setting'inin canli katalog baglami beklentisinden farkli oldugu hipotezini degerlendirir.
// sistemdeki gorevi: PostgreSQL pg_settings LINQ kanitini diger kurallardan ve serbest setting sorgusundan izole eder.
[ExposeServices(typeof(IDiagnosisRule))]
public sealed class ServerSettingMismatchRule : IDiagnosisRule, ITransientDependency
{
    public string HypothesisKindCode => HypothesisKindCodes.ServerSettingMismatch;
    public int Priority => 60;

    // islevi: Resolver izinli setting adi ve katalogdan beklenen degeri uretebildiyse adayi acar.
    public bool AppliesTo(FailureIdentity identity, ResolvedFailureContext context)
        => context.ServerSettingExpectations.Count > 0;

    // islevi: Yalniz resolver'in sabitledigi setting adi ve beklenen katalog degeriyle probe ister.
    public List<ProbeRequest> RequiredProbes(FailureIdentity identity, ResolvedFailureContext context)
    {
        return context.ServerSettingExpectations
            .Select(expectation => new ProbeRequest
            {
                ProbeKindCode = ProbeKindCodes.ServerSetting,
                HypothesisKindCode = HypothesisKindCode,
                SettingName = expectation.Key,
                ExpectedSettingValue = expectation.Value
            })
            .ToList();
    }

    // islevi: Mismatch'i Confirmed, eslesmeyi RuledOut, butce disi probe'u Possible olarak degerlendirir.
    public HypothesisAssessment Assess(
        FailureIdentity identity,
        ResolvedFailureContext context,
        List<ProbeEvidence> evidence)
    {
        var proofs = evidence.Where(item =>
            item.HypothesisKindCode == HypothesisKindCode &&
            item.ProbeKindCode == ProbeKindCodes.ServerSetting).ToList();
        return new HypothesisAssessment(
            HypothesisKindCode,
            Priority,
            ResolveConfidence(proofs),
            proofs);
    }

    // islevi: Herhangi bir mismatch'i Confirmed, tamamlanan tum eslesmeleri RuledOut, eksik butceyi Possible yapar.
    private static string ResolveConfidence(List<ProbeEvidence> proofs)
    {
        if (proofs.Any(item => item.FactCode == ProbeKindCodes.Facts.Mismatch))
        {
            return DiagnosisConfidenceCodes.Confirmed;
        }

        return proofs.Count > 0 && proofs.All(item => item.FactCode == ProbeKindCodes.Facts.Matches)
            ? DiagnosisConfidenceCodes.RuledOut
            : DiagnosisConfidenceCodes.Possible;
    }
}
