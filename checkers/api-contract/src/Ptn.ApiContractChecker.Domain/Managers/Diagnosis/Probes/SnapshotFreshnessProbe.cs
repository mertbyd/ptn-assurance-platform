using Ptn.ApiContractChecker.Constants.Diagnosis;
using Ptn.ApiContractChecker.Interface.Diagnosis;
using Ptn.ApiContractChecker.Models.Diagnosis;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Diagnosis.Probes;

// islevi: Response surum metadatasi ile snapshot info.version degerini karsilastirir.
// sistemdeki gorevi: Deploy-snapshot farkini sabit status kodu veya tarih tahmini olmadan kanitlar.
public sealed class SnapshotFreshnessProbe : IDiagnosisProbe, ITransientDependency
{
    public string ProbeKindCode => ProbeKindCodes.SnapshotFreshness;

    // islevi: Iki surum degeri mevcutsa ordinal esitlik olgusunu uretir.
    public Task<ProbeEvidence> RunAsync(ProbeRequest request, CancellationToken cancellationToken = default)
    {
        var expected = request.Context.Snapshot.ApiVersion;
        var observed = request.Context.Signal.ResponseVersion;
        var fact = expected is null || observed is null
            ? ProbeKindCodes.Facts.Absent
            : string.Equals(expected, observed, StringComparison.Ordinal)
                ? ProbeKindCodes.Facts.Match
                : ProbeKindCodes.Facts.Mismatch;
        return Task.FromResult(new ProbeEvidence
        {
            ProbeKindCode = ProbeKindCode,
            HypothesisKindCode = request.HypothesisKindCode,
            FactCode = fact,
            ExpectedValue = expected,
            ObservedValue = observed
        });
    }
}
