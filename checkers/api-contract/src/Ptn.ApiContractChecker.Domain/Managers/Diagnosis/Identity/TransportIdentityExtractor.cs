using Ptn.ApiContractChecker.Interface.Diagnosis;
using Ptn.ApiContractChecker.Models.Diagnosis;
using Ptn.ApiContractChecker.Models.Snapshots;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Diagnosis.Identity;

// islevi: DNS, TLS, timeout ve baglanti gibi yapilandirilmis transport kodunu HTTP durumundan ayirir.
// sistemdeki gorevi: HTTP response uretilmeyen kesintileri mesaj parse etmeden yuksek guvenli olguya cevirir.
public sealed class TransportIdentityExtractor : IFailureIdentityExtractor, ITransientDependency
{
    public int Priority => 500;

    // islevi: HTTP durumu olmayan yapilandirilmis transport kodunu tanir.
    public bool CanExtract(HttpFailureSignal signal)
        => !signal.StatusCode.HasValue && !string.IsNullOrWhiteSpace(signal.TransportErrorCode);

    // islevi: Transport kodunu kimlige tasiyip yapilandirilmis kaynak olarak yuksek guven verir.
    public void Extract(HttpFailureSignal signal, SpecSnapshotModel snapshot, FailureIdentity identity)
    {
        identity.TransportErrorCode = signal.TransportErrorCode;
        identity.Upgrade();
    }
}
