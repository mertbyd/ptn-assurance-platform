using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ptn.DatabaseChecker.Constants;
using Ptn.DatabaseChecker.ExceptionCodes;
using Ptn.DatabaseChecker.Interface;
using Ptn.DatabaseChecker.Managers.Shared;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace Ptn.DatabaseChecker.Managers;

// islevi: Domain manager'lar icin ortak varlik, benzersizlik ve enum dogrulama yardimcilarini toplar.
// sistemdeki gorevi: Tekil ve toplu validasyonlarda ayni kurallarin tekrar yazilmasini engeller.
public abstract class BaseManager<TEntity> : DatabaseCheckerDomainService
    where TEntity : class, IEntity<Guid>
{
    protected readonly IBaseRepository<TEntity> Repository;

    protected virtual string AlreadyExistsErrorCode => GeneralExceptionCodes.InvalidOperation;
    protected virtual string UpdateNotSupportedErrorCode => GeneralExceptionCodes.UpdateNotSupported;
    protected virtual string DeleteNotSupportedErrorCode => GeneralExceptionCodes.DeleteNotSupported;

    protected BaseManager(
        IBaseRepository<TEntity> repository,
        IAbpLazyServiceProvider abpLazyServiceProvider)
        : base(abpLazyServiceProvider)
    {
        Repository = repository;
    }

    public async Task<TEntity> EnsureExistsAsync(Guid id)
    {
        var entity = await Repository.FindAsync(id);
        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(TEntity), id);
        }

        return entity;
    }

    public virtual async Task<TEntity> RejectUpdateAsync(Guid id)
    {
        await EnsureExistsAsync(id);
        throw new BusinessException(UpdateNotSupportedErrorCode);
    }

    public virtual async Task RejectDeleteAsync(Guid id)
    {
        await EnsureExistsAsync(id);
        throw new BusinessException(DeleteNotSupportedErrorCode);
    }

    public async Task EnsureExistsInAsync<TOther>(
        IRepository<TOther, Guid> otherRepository,
        Guid id)
        where TOther : class, IEntity<Guid>
    {
        var entity = await otherRepository.FindAsync(id);
        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(TOther), id);
        }
    }

    public async Task EnsureExistsInAsync<TOther>(
        IRepository<TOther, Guid> otherRepository,
        Guid? id)
        where TOther : class, IEntity<Guid>
    {
        if (id.HasValue)
        {
            await EnsureExistsInAsync(otherRepository, id.Value);
        }
    }

    public async Task EnsureAllExistInAsync<TOther>(
        IRepository<TOther, Guid> otherRepository,
        IEnumerable<Guid> ids)
        where TOther : class, IEntity<Guid>
    {
        await GetRequiredEntitiesInAsync(otherRepository, ids);
    }

    protected async Task EnsureAllOptionalExistInAsync<TOther>(
        IRepository<TOther, Guid> otherRepository,
        IEnumerable<Guid?> ids)
        where TOther : class, IEntity<Guid>
    {
        var requiredIds = ids
            .Where(id => id.HasValue)
            .Select(id => id!.Value);

        await EnsureAllExistInAsync(otherRepository, requiredIds);
    }

    protected async Task<List<TOther>> GetRequiredEntitiesInAsync<TOther>(
        IRepository<TOther, Guid> otherRepository,
        IEnumerable<Guid> ids)
        where TOther : class, IEntity<Guid>
    {
        var idList = ids.Distinct().ToList();
        if (idList.Count == 0)
        {
            return new List<TOther>();
        }

        if (idList.Contains(Guid.Empty))
        {
            throw new EntityNotFoundException(typeof(TOther), Guid.Empty);
        }

        var foundEntities = await otherRepository.GetListAsync(x => idList.Contains(x.Id));
        EnsureAllIdsFound<TOther>(idList, foundEntities.Select(x => x.Id));

        return foundEntities;
    }

    public async Task EnsureUniqueAsync(Expression<Func<TEntity, bool>> predicate)
    {
        var exists = await Repository.ExistsAsync(predicate);
        if (exists)
        {
            throw new BusinessException(AlreadyExistsErrorCode);
        }
    }

    public async Task EnsureUniqueAsync(
        Expression<Func<TEntity, bool>> predicate,
        Guid excludeId)
    {
        var parameter = predicate.Parameters[0];
        var excludeExpression = Expression.NotEqual(
            Expression.Property(parameter, nameof(IEntity<Guid>.Id)),
            Expression.Constant(excludeId));
        var combinedBody = Expression.AndAlso(predicate.Body, excludeExpression);
        var combinedPredicate = Expression.Lambda<Func<TEntity, bool>>(combinedBody, parameter);

        await EnsureUniqueAsync(combinedPredicate);
    }

    public async Task EnsureUniqueBulkAsync<TValue>(
        IEnumerable<TValue> values,
        Expression<Func<TEntity, TValue>> propertySelector)
    {
        var valueList = PrepareUniqueValues(values);
        if (valueList.Count == 0)
        {
            return;
        }

        var lambda = BuildContainsPredicate(propertySelector, valueList);

        var exists = await Repository.ExistsAsync(lambda);
        if (exists)
        {
            throw new BusinessException(AlreadyExistsErrorCode);
        }
    }

    protected async Task EnsureUniqueBulkAsync<TValue>(
        IEnumerable<TValue> values,
        Expression<Func<TEntity, TValue>> propertySelector,
        Expression<Func<TEntity, bool>> scopePredicate)
    {
        var valueList = PrepareUniqueValues(values);
        if (valueList.Count == 0)
        {
            return;
        }

        var containsPredicate = BuildContainsPredicate(propertySelector, valueList);
        var parameter = containsPredicate.Parameters[0];
        var scopedBody = ReplaceParameter(scopePredicate.Body, scopePredicate.Parameters[0], parameter);
        var combinedBody = Expression.AndAlso(scopedBody, containsPredicate.Body);
        var lambda = Expression.Lambda<Func<TEntity, bool>>(combinedBody, parameter);

        var exists = await Repository.ExistsAsync(lambda);
        if (exists)
        {
            throw new BusinessException(AlreadyExistsErrorCode);
        }
    }

    protected async Task EnsureUniqueKeysForCreateManyAsync<TModel, TKey>(
        List<TModel> models,
        Func<TModel, TKey> modelKeySelector,
        Expression<Func<TEntity, bool>> candidatePredicate,
        Func<TEntity, TKey> entityKeySelector)
    {
        var requestedKeys = PrepareUniqueKeys(models, modelKeySelector);
        if (requestedKeys.Count == 0)
        {
            return;
        }

        var existingEntities = await Repository.GetListAsync(candidatePredicate);
        foreach (var entity in existingEntities)
        {
            if (requestedKeys.Contains(entityKeySelector(entity)))
            {
                throw new BusinessException(AlreadyExistsErrorCode);
            }
        }
    }

    protected async Task<TModel> EnsureUniqueCodeForCreateAsync<TModel>(
        TModel model,
        Func<TModel, string?> modelCodeSelector,
        Expression<Func<TEntity, string>> entityCodeSelector)
    {
        var code = modelCodeSelector(model);
        if (!string.IsNullOrWhiteSpace(code))
        {
            await EnsureUniqueAsync(BuildEqualityPredicate(entityCodeSelector, code));
        }

        return model;
    }

    protected async Task<List<TModel>> EnsureUniqueCodesForCreateManyAsync<TModel>(
        List<TModel> models,
        Func<TModel, string?> modelCodeSelector,
        Expression<Func<TEntity, string>> entityCodeSelector)
    {
        var codes = models
            .Select(modelCodeSelector)
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code!)
            .ToList();

        if (codes.Count > 0)
        {
            await EnsureUniqueBulkAsync(codes, entityCodeSelector);
        }

        return models;
    }

    protected async Task<TModel> EnsureUniqueCodeForUpdateAsync<TModel>(
        TEntity existing,
        TModel model,
        Func<TEntity, string?> entityCodeSelector,
        Func<TModel, string?> modelCodeSelector,
        Expression<Func<TEntity, string>> entityCodeExpression)
    {
        var code = modelCodeSelector(model);
        if (!string.IsNullOrWhiteSpace(code) && entityCodeSelector(existing) != code)
        {
            await EnsureUniqueAsync(BuildEqualityPredicate(entityCodeExpression, code), existing.Id);
        }

        return model;
    }

    protected async Task EnsureValidEnumAsync<TEnum>(TEnum value, string settingName)
        where TEnum : struct, Enum
    {
        var enumValidationManager = LazyGetRequiredService<EnumValidationManager>();
        await enumValidationManager.ValidateAllowedEnumAsync(value, settingName);
    }

    private static Expression<Func<TEntity, bool>> BuildEqualityPredicate(
        Expression<Func<TEntity, string>> propertySelector,
        string value)
    {
        var parameter = propertySelector.Parameters[0];
        var body = Expression.Equal(propertySelector.Body, Expression.Constant(value));
        return Expression.Lambda<Func<TEntity, bool>>(body, parameter);
    }

    protected static void EnsureAllIdsFound<TOther>(IReadOnlyCollection<Guid> expectedIds, IEnumerable<Guid> foundIds)
        where TOther : class, IEntity<Guid>
    {
        var foundIdSet = foundIds.ToHashSet();
        var missingId = expectedIds.FirstOrDefault(id => !foundIdSet.Contains(id));
        if (missingId != Guid.Empty || foundIdSet.Count != expectedIds.Count)
        {
            throw new EntityNotFoundException(typeof(TOther), missingId);
        }
    }

    private List<TValue> PrepareUniqueValues<TValue>(IEnumerable<TValue> values)
    {
        var rawValues = values.Where(v => v != null).ToList();
        var duplicateValue = rawValues
            .GroupBy(v => v)
            .FirstOrDefault(g => g.Count() > 1);

        if (duplicateValue != null)
        {
            throw new BusinessException(AlreadyExistsErrorCode)
                .WithData(BusinessExceptionDataKeys.Value, duplicateValue.Key!);
        }

        return rawValues.Distinct().ToList();
    }

    private HashSet<TKey> PrepareUniqueKeys<TModel, TKey>(
        IEnumerable<TModel> models,
        Func<TModel, TKey> keySelector)
    {
        var keys = models.Select(keySelector).ToList();
        var duplicateKey = keys
            .GroupBy(key => key)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicateKey != null)
        {
            throw new BusinessException(AlreadyExistsErrorCode)
                .WithData(BusinessExceptionDataKeys.Value, duplicateKey.Key!);
        }

        return keys.ToHashSet();
    }

    private static Expression<Func<TEntity, bool>> BuildContainsPredicate<TValue>(
        Expression<Func<TEntity, TValue>> propertySelector,
        List<TValue> valueList)
    {
        var parameter = propertySelector.Parameters[0];
        var containsMethod = typeof(List<TValue>).GetMethod(nameof(List<TValue>.Contains), new[] { typeof(TValue) });
        var containsExpression = Expression.Call(Expression.Constant(valueList), containsMethod!, propertySelector.Body);

        return Expression.Lambda<Func<TEntity, bool>>(containsExpression, parameter);
    }

    private static Expression ReplaceParameter(Expression expression, ParameterExpression source, ParameterExpression target)
    {
        return new ParameterReplaceVisitor(source, target).Visit(expression)!;
    }

    private sealed class ParameterReplaceVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _source;
        private readonly ParameterExpression _target;

        public ParameterReplaceVisitor(ParameterExpression source, ParameterExpression target)
        {
            _source = source;
            _target = target;
        }

        protected override Expression VisitParameter(ParameterExpression node)
            => node == _source ? _target : base.VisitParameter(node);
    }
}
