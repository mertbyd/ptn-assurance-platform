using Ptn.ApiContractChecker.Models.Diagnosis;
using Ptn.ApiContractChecker.Models.Snapshots;

namespace Ptn.ApiContractChecker.Interface.Diagnosis;

// islevi: Tek yapilandirilmis API hata kaynaginin uygulanabilirlik ve kimlik zenginlestirme sozlesmesidir.
// sistemdeki gorevi: Yeni extractor'in resolver veya mevcut extractor dosyalarini degistirmeden DI koleksiyonuna katilmasini saglar.
public interface IFailureIdentityExtractor
{
    int Priority { get; }
    bool CanExtract(HttpFailureSignal signal);
    void Extract(HttpFailureSignal signal, SpecSnapshotModel snapshot, FailureIdentity identity);
}
