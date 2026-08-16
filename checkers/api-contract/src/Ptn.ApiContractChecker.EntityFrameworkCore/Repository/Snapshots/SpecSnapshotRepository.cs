using Microsoft.EntityFrameworkCore;
using Ptn.ApiContractChecker.Constants.Snapshots;
using Ptn.ApiContractChecker.Entities;
using Ptn.ApiContractChecker.Entities.Snapshots;
using Ptn.ApiContractChecker.EntityFrameworkCore;
using Ptn.ApiContractChecker.Interface.Snapshots;
using Ptn.ApiContractChecker.Models.Snapshots;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;

namespace Ptn.ApiContractChecker.Repository.Snapshots;

// islevi: Bir dokumanin anlik goruntu zaman serisini, gecmis sayfalamasini ve navigation'li detay okumasini kurar.
// sistemdeki gorevi: Detayi Include ile tam grafik olarak verirken listeyi ham spec govdesine hic dokunmayan hafif projeksiyonda tutar.
public class SpecSnapshotRepository : BaseRepository<SpecSnapshot>, ISpecSnapshotRepository
{
    private readonly IDataFilter<IPassivable> _passivableFilter;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;

    public SpecSnapshotRepository(
        IDbContextProvider<ApiContractCheckerDbContext> provider,
        IDataFilter<IPassivable> passivableFilter,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser)
        : base(provider)
    {
        _passivableFilter = passivableFilter;
        _currentTenant = currentTenant;
        _currentUser = currentUser;
    }

    // Dokumanin en son acilan anlik goruntusunu getirir.
    public async Task<SpecSnapshot?> FindLatestForDocumentAsync(Guid specDocumentId)
    {
        var query = await GetQueryableAsync();
        return await query
            .Where(snapshot => snapshot.SpecDocumentId == specDocumentId)
            .OrderByDescending(snapshot => snapshot.CreationTime)
            .FirstOrDefaultAsync();
    }

    // Dokumanin gecmisini ham govde disinda kalan baslik kolonlariyla CreationTime ve Id azalan sirada sayfalar.
    public async Task<List<SpecSnapshotHeaderModel>> GetPagedHeadersForDocumentAsync(
        Guid specDocumentId,
        int skipCount,
        int maxResultCount)
    {
        // Emekli edilmis bir format lookup'i gecmisi silmemeli; snapshot satiri kendi formatiyla anlatilmaya devam eder.
        using (_passivableFilter.Disable())
        {
            var query = await BuildHeaderQueryAsync(specDocumentId);
            return await query
                .OrderByDescending(header => header.CreationTime)
                .ThenByDescending(header => header.Id)
                .Skip(skipCount)
                .Take(maxResultCount)
                .ToListAsync();
        }
    }

    // Listeyle ayni dokuman filtresinde gorulebilir snapshot sayisini hesaplar.
    public async Task<long> GetHeaderCountForDocumentAsync(Guid specDocumentId)
    {
        var query = await GetQueryableAsync();
        return await query
            .Where(snapshot => snapshot.SpecDocumentId == specDocumentId)
            .LongCountAsync();
    }

    // Tek snapshot'i degismez icerigi ve format satiriyla birlikte tam grafik olarak getirir.
    public async Task<SpecSnapshot?> FindWithDetailsAsync(Guid id)
    {
        // Format lookup'i pasife alinsa bile mevcut snapshot detayi okunabilir kalmalidir.
        using (_passivableFilter.Disable())
        {
            var query = await BuildAccessibleQueryAsync();
            return await query
                .AsNoTracking()
                .Include(snapshot => snapshot.SpecContent)
                .Include(snapshot => snapshot.SpecFormat)
                .FirstOrDefaultAsync(snapshot => snapshot.Id == id);
        }
    }

    // ABP tenant filtresini host kullanicisinin kendi ve sistem snapshot'lariyla tamamlar.
    // Yalniz kaynak kapisindan gecmeyen tekil detay yolu kullanir; gecmis listesi gorunurlugu
    // zaten SpecSource uzerinden alir, cekim akisi ise kullanici ayrimi yapmadan son snapshot'i gormelidir.
    private async Task<IQueryable<SpecSnapshot>> BuildAccessibleQueryAsync()
    {
        var query = await GetQueryableAsync();
        if (_currentTenant.Id.HasValue)
        {
            return query;
        }

        var userId = _currentUser.Id;
        return query.Where(snapshot => snapshot.CreatorId == null || snapshot.CreatorId == userId);
    }

    // Dokumanin snapshot sorgusunu navigation'lar uzerinden ham govde tasimayan hafif baslik modeline projekte eder.
    private async Task<IQueryable<SpecSnapshotHeaderModel>> BuildHeaderQueryAsync(Guid specDocumentId)
    {
        var snapshots = (await GetQueryableAsync()).AsNoTracking();
        return snapshots
            .Where(snapshot => snapshot.SpecDocumentId == specDocumentId)
            .Select(snapshot => new SpecSnapshotHeaderModel
            {
                Id = snapshot.Id,
                CreationTime = snapshot.CreationTime,
                LastSeenAt = snapshot.LastSeenAt,
                FormatCode = snapshot.SpecFormat.Code,
                ApiVersion = snapshot.ApiVersion,
                ByteSize = snapshot.SpecContent.ByteSize,
                ShortCanonicalHash =
                    snapshot.SpecContent.CanonicalHash.Substring(0, SpecSnapshotConsts.ShortHashLength)
            });
    }
}
