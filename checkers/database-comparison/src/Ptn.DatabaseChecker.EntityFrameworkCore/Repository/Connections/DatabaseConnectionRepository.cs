using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ptn.DatabaseChecker.Entities.Connections;
using Ptn.DatabaseChecker.EntityFrameworkCore;
using Ptn.DatabaseChecker.Interface.Connections;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;

namespace Ptn.DatabaseChecker.Repository.Connections;

// islevi: DatabaseConnection sorgularini Engine lookup'u Include'uyla calistirir.
// sistemdeki gorevi: DTO'daki EngineCode/EngineName alanlarini tek sorguda besler; LINQ yalnizca burada yasar.
public class DatabaseConnectionRepository : BaseRepository<DatabaseConnection>, IDatabaseConnectionRepository
{
    // islevi: Sorgunun aktif tenant ve kullanici baglamini okur.
    // sistemdeki gorevi: ABP'nin global tenant filtresini, tenant + CreatorId gorunurluk kuraliyla tamamlar.
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;

    public DatabaseConnectionRepository(
        IDbContextProvider<DatabaseCheckerDbContext> dbContextProvider,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser)
        : base(dbContextProvider)
    {
        _currentTenant = currentTenant;
        _currentUser = currentUser;
    }

    public Task<DatabaseConnection?> FindWithDetailsAsync(Guid id)
        => FindWithDetailsAsync(id, default);

    public Task<DatabaseConnection> GetWithDetailsAsync(Guid id)
        => GetWithDetailsAsync(id, default);

    public async Task<DatabaseConnection?> FindWithDetailsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = await BuildAccessibleQueryAsync();
        return await query
            .Include(x => x.Engine)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    // islevi: Gorulebilir baglantiyi Engine navigation'iyla okur ve eksik kaydi ABP not-found davranisina cevirir.
    public async Task<DatabaseConnection> GetWithDetailsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
        => await FindWithDetailsAsync(id, cancellationToken)
           ?? throw new EntityNotFoundException(typeof(DatabaseConnection), id);

    // islevi: Background worker baglantisini CurrentUser olmadan, aktif tenant siniriyla okur.
    // sistemdeki gorevi: Job payload'indaki tenant contexti (DatabaseCheckerTenantBackgroundJob CurrentTenant.Change) ABP IMultiTenant global filtresini kurar; elle TenantId filtresi yazilmaz. User-owned ve system-owned baglantilarin ikisini de aktif tenant kapsaminda cozer.
    public async Task<DatabaseConnection?> FindForExecutionAsync(Guid id)
    {
        var query = await GetQueryableAsync();
        return await query
            .Include(connection => connection.Engine)
            .FirstOrDefaultAsync(connection => connection.Id == id);
    }

    public async Task<List<DatabaseConnection>> GetPagedWithDetailsAsync(int skipCount, int maxResultCount)
    {
        var query = await BuildAccessibleQueryAsync();
        return await query
            .Include(x => x.Engine)
            .OrderBy(x => x.Name)
            .Skip(skipCount)
            .Take(maxResultCount)
            .ToListAsync();
    }

    public async Task<List<DatabaseConnection>> GetWithDetailsByIdsAsync(
        List<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        var query = await BuildAccessibleQueryAsync();
        return await query
            .Include(x => x.Engine)
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(cancellationToken);
    }

    public Task<List<DatabaseConnection>> GetWithDetailsByIdsAsync(List<Guid> ids)
        => GetWithDetailsByIdsAsync(ids, default);

    public async Task<List<DatabaseConnection>> GetAccessibleByIdsAsync(List<Guid> ids)
    {
        var query = await BuildAccessibleQueryAsync();
        return await query
            .Where(x => ids.Contains(x.Id))
            .ToListAsync();
    }

    public async Task<long> GetAccessibleCountAsync()
    {
        var query = await BuildAccessibleQueryAsync();
        return await query.LongCountAsync();
    }

    // islevi: DatabaseConnection sorgusunu aktif kullanicinin gorus alanina indirger.
    // sistemdeki gorevi: Tenant kapsamini ABP IMultiTenant global filtresi uygular (elle TenantId yazilmaz); yalniz host/tenant'siz baglamda ABP'nin vermedigi bireysel (CreatorId) sahiplik kisiti eklenir.
    private async Task<IQueryable<DatabaseConnection>> BuildAccessibleQueryAsync()
    {
        // ABP tenant filtresi: tenant kullanicisi -> yalniz kendi tenant satirlari, host -> yalniz TenantId == null satirlari.
        var query = await GetQueryableAsync();

        // Tenant baglami: tenant paylasimli gorunurluk (kural: tenant DB'leri = tenantId); ek sahiplik kisiti yok.
        if (_currentTenant.Id.HasValue)
        {
            return query;
        }

        // Host/tenant'siz baglam: bireysel (userid) sahiplik — kullanicinin kendi kayitlari + CreatorId'si olmayan sistem-seed kayitlari.
        var userId = _currentUser.Id;
        return query.Where(connection => connection.CreatorId == null || connection.CreatorId == userId);
    }
}
