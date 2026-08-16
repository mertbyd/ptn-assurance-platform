using Ptn.ApiContractChecker.Interface.Diagnosis;
using Ptn.ApiContractChecker.Models.Diagnosis;
using Ptn.ApiContractChecker.Models.Snapshots;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Diagnosis.Identity;

// islevi: KBP-621 conformance outcome kodunu yapilandirilmis teshis kimligine tasir.
// sistemdeki gorevi: Oracle basarisizligini response metni veya status kodu tahmini olmadan hipotezlere baglar.
public sealed class ConformanceIdentityExtractor : IFailureIdentityExtractor, ITransientDependency
{
    public int Priority => 400;

    // islevi: Sinyalde conformance outcome kodu bulunup bulunmadigini bildirir.
    public bool CanExtract(HttpFailureSignal signal)
        => !string.IsNullOrWhiteSpace(signal.ConformanceOutcomeCode);

    // islevi: Outcome kodunu yuksek guvenli kimlik olgusu olarak kaydeder.
    public void Extract(HttpFailureSignal signal, SpecSnapshotModel snapshot, FailureIdentity identity)
    {
        identity.ConformanceOutcomeCode = signal.ConformanceOutcomeCode;
        identity.Upgrade();
    }
}
