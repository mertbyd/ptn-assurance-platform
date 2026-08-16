using Ptn.ApiContractChecker.Entities.Snapshots;
using Ptn.ApiContractChecker.Models.Snapshots;

namespace Ptn.ApiContractChecker.Interface.Snapshots;

// islevi: Bir dokumanin zaman serisindeki anlik goruntu, gecmis sayfalama ve tekil detay sorgularini tanimlar.
// sistemdeki gorevi: Cekim karar akisinin "son gorulen icerik" bilgisini ve okuma yuzeyinin sorgularini ayni kontratta toplar.
public interface ISpecSnapshotRepository : IBaseRepository<SpecSnapshot>
{
    // Dokumanin en son alinan anlik goruntusunu getirir; hic snapshot yoksa null doner.
    Task<SpecSnapshot?> FindLatestForDocumentAsync(Guid specDocumentId);

    // Dokumanin snapshot gecmisini ham govde olmadan CreationTime azalan sirada sayfalar.
    Task<List<SpecSnapshotHeaderModel>> GetPagedHeadersForDocumentAsync(
        Guid specDocumentId,
        int skipCount,
        int maxResultCount);

    // Listeyle ayni dokuman filtresinde gorulebilir snapshot sayisini hesaplar.
    Task<long> GetHeaderCountForDocumentAsync(Guid specDocumentId);

    // Tek snapshot'i bagli icerik ve format satirlariyla birlikte getirir.
    Task<SpecSnapshot?> FindWithDetailsAsync(Guid id);
}
