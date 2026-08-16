using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
namespace Ptn.ApiContractChecker.Interface;
// islevi: Tum custom repository interface'leri icin ortak kontrati tanimlar.
// sistemdeki gorevi: Varlik, benzersizlik ve bulk islemlerini tek repository tabaninda standartlastirir.
public interface IBaseRepository<TEntity> : IRepository<TEntity, Guid>
    where TEntity : class, IEntity<Guid>
{
    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate);
    Task<List<TEntity>> InsertManyAndGetListAsync(IEnumerable<TEntity> entities);
    Task SoftDeleteAsync(Guid id);
    Task SoftDeleteManyAsync(IEnumerable<Guid> ids);
}
