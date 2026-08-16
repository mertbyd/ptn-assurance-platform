using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ptn.ApiContractChecker.ExceptionCodes;
using Ptn.ApiContractChecker.Interface;
using Ptn.ApiContractChecker.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Ptn.ApiContractChecker.Repository;

// islevi: Tum EF repository'leri icin ortak CRUD, exists ve soft-delete yardimcilarini saglar.
// sistemdeki gorevi: Repository katmaninda ayni sorgu ve bulk davranislarinin tekrar yazilmasini engeller.
public class BaseRepository<TEntity> : EfCoreRepository<ApiContractCheckerDbContext, TEntity, Guid>, IBaseRepository<TEntity>
    where TEntity : class, IEntity<Guid>
{
    public BaseRepository(IDbContextProvider<ApiContractCheckerDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.AnyAsync(predicate);
    }

    public async Task<List<TEntity>> InsertManyAndGetListAsync(IEnumerable<TEntity> entities)
    {
        var list = entities.ToList();
        await InsertManyAsync(list, autoSave: true);
        return list;
    }

    public async Task SoftDeleteAsync(Guid id)
    {
        if (!typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
        {
            throw new BusinessException(GeneralExceptionCodes.SoftDeleteNotSupported);
        }

        await DeleteAsync(id, autoSave: true);
    }

    public async Task SoftDeleteManyAsync(IEnumerable<Guid> ids)
    {
        if (!typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
        {
            throw new BusinessException(GeneralExceptionCodes.SoftDeleteNotSupported);
        }

        await DeleteManyAsync(ids, autoSave: true);
    }
}
