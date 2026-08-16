using System.Collections.Generic;
using Ptn.DatabaseChecker.Constants.Diagnosis;
using Ptn.DatabaseChecker.Interface.Diagnosis;
using Ptn.DatabaseChecker.Models.Diagnosis;
using Volo.Abp.DependencyInjection;

namespace Ptn.DatabaseChecker.Managers.Diagnosis.Rules;

// islevi: Hata kolonu veya failed expectation kolonunun generated/computed oldugu yazma hipotezini degerlendirir.
// sistemdeki gorevi: KBP-703 IsGenerated olgusunu provider mesajini parse etmeden Likely kanita cevirir.
[ExposeServices(typeof(IDiagnosisRule))]
public sealed class GeneratedColumnWriteRule : IDiagnosisRule, ITransientDependency
{
    public string HypothesisKindCode => HypothesisKindCodes.GeneratedColumnWrite;
    public int Priority => 94;

    // islevi: Canli katalogda cozulmus kolon generated/computed ise adayi acar.
    public bool AppliesTo(FailureIdentity identity, ResolvedFailureContext context)
        => identity.IndicatesGeneratedColumnWrite && context.Column?.IsGenerated == true;

    // islevi: Generated kolon katalog olgusu yeterli oldugu icin ek hedef probe istemez.
    public List<ProbeRequest> RequiredProbes(FailureIdentity identity, ResolvedFailureContext context)
        => new();

    // islevi: Katalog IsGenerated olgusunu yazma niyeti dogrudan gozlenmedigi icin Likely olarak raporlar.
    public HypothesisAssessment Assess(
        FailureIdentity identity,
        ResolvedFailureContext context,
        List<ProbeEvidence> evidence)
        => new(
            HypothesisKindCode,
            Priority,
            DiagnosisConfidenceCodes.Likely,
            new() { ProbeEvidence.Catalog(HypothesisKindCode) });
}
