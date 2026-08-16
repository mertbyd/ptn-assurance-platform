using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ptn.DatabaseChecker.Entities.Definitions;
using Ptn.DatabaseChecker.EntityFrameworkCore;
using Ptn.DatabaseChecker.Interface.Definitions;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;

namespace Ptn.DatabaseChecker.Repository.Definitions;

// islevi: ComparisonDefinition sorgularini baglanti/mod Include'lariyla calistirir.
// sistemdeki gorevi: Tarif listesi kaynak/hedef baglanti adlari ve moduyla tek sorguda beslenir; kapsam kurallari gomulu owned jsonb oldugu icin ayrica Include gerektirmez.
public class ComparisonDefinitionRepository : BaseRepository<ComparisonDefinition>, IComparisonDefinitionRepository
{
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;

    public ComparisonDefinitionRepository(
        IDbContextProvider<DatabaseCheckerDbContext> dbContextProvider,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser)
        : base(dbContextProvider)
    {
        _currentTenant = currentTenant;
        _currentUser = currentUser;
    }

    public async Task<ComparisonDefinition?> FindWithDetailsAsync(Guid id)
    {
        var query = await BuildAccessibleQueryAsync();
        return await BuildQuery(query).FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<ComparisonDefinition>> GetPagedWithDetailsAsync(int skipCount, int maxResultCount)
    {
        var query = await BuildAccessibleQueryAsync();
        return await BuildQuery(query)
            .OrderBy(x => x.Name)
            .Skip(skipCount)
            .Take(maxResultCount)
            .ToListAsync();
    }

    public async Task<List<ComparisonDefinition>> GetWithDetailsByIdsAsync(List<Guid> ids)
    {
        var query = await BuildAccessibleQueryAsync();
        return await BuildQuery(query)
            .Where(x => ids.Contains(x.Id))
            .ToListAsync();
    }

    public async Task<List<ComparisonDefinition>> GetAccessibleByIdsAsync(List<Guid> ids)
    {
        var query = await BuildAccessibleQueryAsync();
        return await query.Where(definition => ids.Contains(definition.Id)).ToListAsync();
    }

    public async Task<long> GetAccessibleCountAsync()
    {
        var query = await BuildAccessibleQueryAsync();
        return await query.LongCountAsync();
    }

    // Tarifin uc navigation'inin tek Include tanimi; tekil ve sayfali okuma paylasir (is bir kez yapilir). Kapsam kurallari owned jsonb oldugu icin Include'a girmez.
    private static IQueryable<ComparisonDefinition> BuildQuery(IQueryable<ComparisonDefinition> query)
    {
        return query
            .Include(x => x.SourceConnection)
            .Include(x => x.TargetConnection)
            .Include(x => x.ComparisonType);
    }

    // ABP tenant filtresini host kullanicisinin kendi ve sistem tarifleriyle tamamlar.
    private async Task<IQueryable<ComparisonDefinition>> BuildAccessibleQueryAsync()
    {
        var query = await GetQueryableAsync();
        if (_currentTenant.Id.HasValue)
        {
            return query;
        }

        var userId = _currentUser.Id;
        if (userId is null)
        {
            // Background execution kullanici claim'i tasimaz; tenant filtresi yine aktiftir.
            return query;
        }

        return query.Where(definition => definition.CreatorId == null || definition.CreatorId == userId);
    }
}
