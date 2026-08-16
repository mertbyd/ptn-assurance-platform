using Ptn.ApiContractChecker.Entities.Sources;
using Ptn.ApiContractChecker.Models.Sources;

namespace Ptn.ApiContractChecker.Interface.Sources;

// islevi: SpecSource aggregate'inin dokumanli ve erisim-kapsamli sorgu kontratini tanimlar.
// sistemdeki gorevi: Tum LINQ, sayfalama ve host kullanicisi gorunurlugunu EF repository uygulamasinda tutar.
public interface ISpecSourceRepository : IBaseRepository<SpecSource>
{
    // Tek kaynagi aktif dokumanlariyla ve gorunurluk kuraliyla getirir.
    Task<SpecSource?> FindWithDetailsAsync(Guid id);

    // Kaynaklari aktif dokumanlariyla veritabani tarafinda sayfalar.
    Task<List<SpecSource>> GetPagedWithDetailsAsync(int skipCount, int maxResultCount);

    // Verilen kaynaklari aggregate dokumanlariyla tek sorguda getirir.
    Task<List<SpecSource>> GetWithDetailsByIdsAsync(List<Guid> ids);

    // Aktif kullanicinin gorebildigi toplam kaynak sayisini hesaplar.
    Task<long> GetAccessibleCountAsync();

    // Vadesi gelmis izlenen dokumanlari butun tenant'lar boyunca tek sorguda getirir.
    Task<List<DueSpecDocumentModel>> GetDueMonitoredDocumentsAsync(DateTime dueAt, int maxCount);
}
