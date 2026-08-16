using Microsoft.EntityFrameworkCore;
using Ptn.ApiContractChecker.Entities.Sources;
using Ptn.ApiContractChecker.EntityFrameworkCore;
using Ptn.ApiContractChecker.Interface.Sources;
using Ptn.ApiContractChecker.Models.Sources;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;

namespace Ptn.ApiContractChecker.Repository.Sources;

// islevi: SpecSource icin dokumanli detay, sayfalama ve gorunurluk sorgularini kurar.
// sistemdeki gorevi: Tenant filtresini ABP'ye birakir; yalniz host baglamindaki kullanici sahipligini tek LINQ merkezinde tamamlar.
public class SpecSourceRepository : BaseRepository<SpecSource>, ISpecSourceRepository
{
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;
    private readonly IDataFilter<IMultiTenant> _multiTenantFilter;

    public SpecSourceRepository(
        IDbContextProvider<ApiContractCheckerDbContext> provider,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser,
        IDataFilter<IMultiTenant> multiTenantFilter)
        : base(provider)
    {
        _currentTenant = currentTenant;
        _currentUser = currentUser;
        _multiTenantFilter = multiTenantFilter;
    }

    // Tek kaynagi gorunurluk kurali ve aktif aggregate dokumanlariyla getirir.
    public async Task<SpecSource?> FindWithDetailsAsync(Guid id)
    {
        var query = await BuildAccessibleQueryAsync();
        return await query
            .Include(source => source.Documents)
            .FirstOrDefaultAsync(source => source.Id == id);
    }

    // Kaynaklari ad sirasinda ve veritabani tarafinda aktif dokumanlariyla sayfalar.
    public async Task<List<SpecSource>> GetPagedWithDetailsAsync(int skipCount, int maxResultCount)
    {
        var query = await BuildAccessibleQueryAsync();
        return await query
            .Include(source => source.Documents)
            .OrderBy(source => source.Name)
            .Skip(skipCount)
            .Take(maxResultCount)
            .ToListAsync();
    }

    // Verilen kaynaklari gorunurluk kurali ve aggregate dokumanlariyla tek sorguda getirir.
    public async Task<List<SpecSource>> GetWithDetailsByIdsAsync(List<Guid> ids)
    {
        var query = await BuildAccessibleQueryAsync();
        return await query
            .Include(source => source.Documents)
            .Where(source => ids.Contains(source.Id))
            .ToListAsync();
    }

    // Liste metadata sayisini satir listesiyle ayni gorunurluk sorgusundan hesaplar.
    public async Task<long> GetAccessibleCountAsync()
    {
        var query = await BuildAccessibleQueryAsync();
        return await query.LongCountAsync();
    }

    // Vadesi gelmis izlenen dokumanlari butun tenant'lar boyunca tek projeksiyon sorgusunda getirir.
    public async Task<List<DueSpecDocumentModel>> GetDueMonitoredDocumentsAsync(DateTime dueAt, int maxCount)
    {
        // Worker CurrentTenant olmadan calisir; ABP IMultiTenant filtresi bu sorguyu "TenantId == null"a
        // indirger ve tarama bosalir. Capraz tenant okuma yalniz bu sorgunun omru boyunca ve yalniz tenant
        // filtresi icin acilir; global filtreleri toptan kaldiran EF yolu kullanilmaz, boylece IPassivable
        // acik kalir ve pasif kaynak/dokuman satirlari taramaya girmez.
        using (_multiTenantFilter.Disable())
        {
            var query = await GetQueryableAsync();
            return await query
                .SelectMany(source => source.Documents)
                .Where(document => document.IsMonitored && document.NextCheckAt <= dueAt)
                .OrderBy(document => document.NextCheckAt)
                .ThenBy(document => document.Id)
                .Take(maxCount)
                .Select(document => new DueSpecDocumentModel
                {
                    SpecSourceId = document.SpecSourceId,
                    SpecDocumentId = document.Id,
                    TenantId = document.TenantId
                })
                .ToListAsync();
        }
    }

    // ABP tenant filtresini host kullanicisinin kendi ve sistem kayitlariyla tamamlar.
    private async Task<IQueryable<SpecSource>> BuildAccessibleQueryAsync()
    {
        var query = await GetQueryableAsync();
        if (_currentTenant.Id.HasValue)
        {
            return query;
        }

        var userId = _currentUser.Id;

        // Kullanici yoksa cagiran HTTP istegi degil sistemdir (zamanlanmis kontrol job'i): host kullanici
        // filtresi uygulanmaz. Aksi halde filtre "CreatorId == null"a indirgenir ve bir kullanicinin kurdugu
        // host kaynagi job'a hic gorunmez; run repository'sinde ayni tuzak KBP-617 canli turunda yasandi.
        // Anonim HTTP yolu yoktur: tum kaynak endpointleri Sources.View izni ister.
        if (userId is null)
        {
            return query;
        }

        return query.Where(source => source.CreatorId == null || source.CreatorId == userId);
    }
}
