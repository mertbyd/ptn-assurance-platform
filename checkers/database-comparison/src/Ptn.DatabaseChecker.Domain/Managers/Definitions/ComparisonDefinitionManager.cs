using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ptn.DatabaseChecker.Entities.Connections;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Entities.Definitions;
using Ptn.DatabaseChecker.Entities.Lookups;
using Ptn.DatabaseChecker.ExceptionCodes;
using Ptn.DatabaseChecker.Interface.Connections;
using Ptn.DatabaseChecker.Interface.Definitions;
using Ptn.DatabaseChecker.Models.Definitions;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace Ptn.DatabaseChecker.Managers.Definitions;

// islevi: Karsilastirma tanimi ad benzersizligi ve baglanti/mod FK kurallarini isletir.
// sistemdeki gorevi: Tarif CRUD akisinda ad ve referans is kurallarini manager katmaninda tutar.
public class ComparisonDefinitionManager : BaseManager<ComparisonDefinition>
{
    // Tarif ad benzersizligi ihlalinde kullanilacak hata kodu.
    protected override string AlreadyExistsErrorCode => ComparisonDefinitionExceptionCodes.NameAlreadyExists;

    // Baglanti FK varliklari tipli repository uzerinden dogrulanir.
    private IDatabaseConnectionRepository ConnectionRepository => LazyGetRequiredService<IDatabaseConnectionRepository>();

    // ComparisonType FK varligi lookup repository uzerinden dogrulanir.
    private IRepository<ComparisonType, Guid> ComparisonTypeRepository => LazyGetRequiredService<IRepository<ComparisonType, Guid>>();

    private ICurrentUser CurrentUser => LazyGetRequiredService<ICurrentUser>();

    public ComparisonDefinitionManager(
        IComparisonDefinitionRepository repository,
        IAbpLazyServiceProvider abpLazyServiceProvider)
        : base(repository, abpLazyServiceProvider)
    {
    }

    // islevi: Yeni tarif modelinde ad tekilligi ve baglanti/mod FK varliklarini dogrular.
    public async Task<CreateComparisonDefinitionModel> ValidateCreateAsync(CreateComparisonDefinitionModel model)
    {
        EnsureSourceRole(model.SourceRoleCode);
        await EnsureNameUniqueForCreateAsync(model.Name);
        await EnsureReferencesExistAsync(model.SourceConnectionId, model.TargetConnectionId, model.ComparisonTypeId);
        return model;
    }

    // islevi: Toplu tarif olusturmada ad tekilligi ve FK varliklarini tekil sorgularla dogrular.
    public async Task<List<CreateComparisonDefinitionModel>> ValidateCreateManyAsync(List<CreateComparisonDefinitionModel> models)
    {
        models.ForEach(model => EnsureSourceRole(model.SourceRoleCode));
        await EnsureNamesUniqueForCreateManyAsync(models);
        await EnsureConnectionsExistAsync(models.SelectMany(x => new[] { x.SourceConnectionId, x.TargetConnectionId }));
        await EnsureComparisonTypesExistAsync(models.Select(x => x.ComparisonTypeId));
        return models;
    }

    // islevi: Tarif guncellemesinde ad degistiyse benzersizligi ve baglanti/mod FK varliklarini dogrular.
    public async Task<UpdateComparisonDefinitionModel> ValidateUpdateAsync(ComparisonDefinition existing, UpdateComparisonDefinitionModel model)
    {
        EnsureSourceRole(model.SourceRoleCode);
        await EnsureNameUniqueForUpdateAsync(existing, model.Name);
        await EnsureReferencesExistAsync(model.SourceConnectionId, model.TargetConnectionId, model.ComparisonTypeId);
        return model;
    }

    // islevi: Domain cagiranlarinda da kaynak rolunun kapali katalogdan gelmesini garanti eder.
    private static void EnsureSourceRole(string sourceRoleCode)
    {
        if (!ComparisonSideRoleCodes.IsDefined(sourceRoleCode))
        {
            throw new BusinessException(ComparisonDefinitionExceptionCodes.Validation.SourceRoleCodeInvalid);
        }
    }

    // islevi: Tenant veya host-kullanici gorunurluk kapsaminda yeni tarif adinin bosta oldugunu dogrular.
    private async Task EnsureNameUniqueForCreateAsync(string name)
    {
        var scope = BuildVisibilityScopePredicate();
        var parameter = scope.Parameters[0];
        var body = Expression.AndAlso(
            scope.Body,
            Expression.Equal(Expression.Property(parameter, nameof(ComparisonDefinition.Name)), Expression.Constant(name)));
        await EnsureUniqueAsync(Expression.Lambda<Func<ComparisonDefinition, bool>>(body, parameter));
    }

    // islevi: Toplu create'te tarif adlarini istek-ici ve DB tekrarlarina karsi dogrular.
    private async Task EnsureNamesUniqueForCreateManyAsync(List<CreateComparisonDefinitionModel> models)
    {
        await EnsureUniqueBulkAsync(
            models.Select(x => x.Name),
            x => x.Name,
            BuildVisibilityScopePredicate());
    }

    // islevi: Guncellemede ad degismediyse sorgu yapmadan, degistiyse mevcut kayit haric tekilligi dogrular.
    private async Task EnsureNameUniqueForUpdateAsync(ComparisonDefinition existing, string name)
    {
        if (existing.Name != name)
        {
            var scope = BuildVisibilityScopePredicate();
            var parameter = scope.Parameters[0];
            var body = Expression.AndAlso(
                scope.Body,
                Expression.Equal(Expression.Property(parameter, nameof(ComparisonDefinition.Name)), Expression.Constant(name)));
            await EnsureUniqueAsync(Expression.Lambda<Func<ComparisonDefinition, bool>>(body, parameter), existing.Id);
        }
    }

    private Expression<Func<ComparisonDefinition, bool>> BuildVisibilityScopePredicate()
    {
        if (CurrentTenant.Id.HasValue)
        {
            var tenantId = CurrentTenant.Id.Value;
            return definition => definition.TenantId == tenantId;
        }

        var userId = CurrentUser.Id;
        return definition => definition.TenantId == null && definition.CreatorId == userId;
    }

    // islevi: Tarifin baglanti ve mod referanslarini dogrular.
    private async Task EnsureReferencesExistAsync(Guid sourceConnectionId, Guid targetConnectionId, Guid comparisonTypeId)
    {
        await EnsureConnectionsExistAsync(new[] { sourceConnectionId, targetConnectionId });
        await EnsureExistsInAsync(ComparisonTypeRepository, comparisonTypeId);
    }

    // islevi: Baglanti referanslarini tek sorguyla dogrular.
    private async Task EnsureConnectionsExistAsync(IEnumerable<Guid> connectionIds)
    {
        var ids = connectionIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return;
        }

        if (ids.Contains(Guid.Empty))
        {
            throw new EntityNotFoundException(typeof(DatabaseConnection), Guid.Empty);
        }

        var foundConnections = await ConnectionRepository.GetAccessibleByIdsAsync(ids);
        EnsureAllIdsFound<DatabaseConnection>(ids, foundConnections.Select(x => x.Id));
    }

    // islevi: ComparisonType lookup referanslarini tek sorguyla dogrular.
    private async Task EnsureComparisonTypesExistAsync(IEnumerable<Guid> comparisonTypeIds)
    {
        await EnsureAllExistInAsync(ComparisonTypeRepository, comparisonTypeIds);
    }
}
