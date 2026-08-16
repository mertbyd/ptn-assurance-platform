using System;
using System.Linq;
using System.Threading.Tasks;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Entities.Connections;
using Ptn.DatabaseChecker.Entities.Definitions;
using Ptn.DatabaseChecker.Entities.Lookups;
using Ptn.DatabaseChecker.Entities.Runs;
using Ptn.DatabaseChecker.Interface.Runs;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.Runs;

// islevi: Pasif referanslara bagli kalici run kayitlarinin sayim, liste ve detay sorgularinda birlikte gorunmesini pinler.
// sistemdeki gorevi: Gecmis sorgularinda IPassivable filtresi kapatilirken ABP tenant filtresinin acik kalmasini kalici guvenceye alir.
public class ComparisonRunHistoryRepository_Tests : DatabaseCheckerEntityFrameworkCoreTestBase
{
    private readonly IComparisonRunRepository _runRepository;
    private readonly IRepository<DatabaseEngine, Guid> _databaseEngineRepository;
    private readonly IRepository<ComparisonType, Guid> _comparisonTypeRepository;
    private readonly IRepository<ComparisonRunStatus, Guid> _runStatusRepository;
    private readonly IRepository<DatabaseConnection, Guid> _connectionRepository;
    private readonly IRepository<ComparisonDefinition, Guid> _definitionRepository;
    private readonly ICurrentTenant _currentTenant;
    private readonly IGuidGenerator _guidGenerator;

    public ComparisonRunHistoryRepository_Tests()
    {
        _runRepository = GetRequiredService<IComparisonRunRepository>();
        _databaseEngineRepository = GetRequiredService<IRepository<DatabaseEngine, Guid>>();
        _comparisonTypeRepository = GetRequiredService<IRepository<ComparisonType, Guid>>();
        _runStatusRepository = GetRequiredService<IRepository<ComparisonRunStatus, Guid>>();
        _connectionRepository = GetRequiredService<IRepository<DatabaseConnection, Guid>>();
        _definitionRepository = GetRequiredService<IRepository<ComparisonDefinition, Guid>>();
        _currentTenant = GetRequiredService<ICurrentTenant>();
        _guidGenerator = GetRequiredService<IGuidGenerator>();
    }

    [Fact]
    public async Task History_Reads_Should_Include_Passive_References_Without_Leaking_Other_Tenants()
    {
        var engineId = await GetLookupIdAsync(_databaseEngineRepository, DatabaseEngineCodes.PostgreSql);
        var comparisonTypeId = await GetLookupIdAsync(_comparisonTypeRepository, ComparisonTypeCodes.SchemaOnly);
        var completedStatusId = await GetLookupIdAsync(_runStatusRepository, ComparisonRunStatusCodes.Completed);
        var ownerTenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        var graph = await CreateRunGraphAsync(ownerTenantId, engineId, comparisonTypeId, completedStatusId);

        await PassivateReferencesAsync(ownerTenantId, graph.SourceConnectionId, graph.TargetConnectionId, graph.DefinitionId);

        await WithUnitOfWorkAsync(async () =>
        {
            using (_currentTenant.Change(ownerTenantId))
            {
                var count = await _runRepository.GetCountByDefinitionAsync(null);
                var headers = await _runRepository.GetPagedHeadersAsync(null, 0, 10);
                var detail = await _runRepository.FindWithDetailsAsync(graph.RunId);

                count.ShouldBe(1);
                headers.Count.ShouldBe(1);
                headers.Single().SourceConnectionName.ShouldBe("history-source");
                headers.Single().TargetConnectionName.ShouldBe("history-target");
                headers.Single().ComparisonDefinitionName.ShouldBe("history-definition");
                detail.ShouldNotBeNull();
                detail.SourceConnection.IsActive.ShouldBeFalse();
                detail.TargetConnection.IsActive.ShouldBeFalse();
                detail.Definition.ShouldNotBeNull();
                detail.Definition.IsActive.ShouldBeFalse();
            }

            using (_currentTenant.Change(otherTenantId))
            {
                (await _runRepository.GetCountByDefinitionAsync(null)).ShouldBe(0);
                (await _runRepository.GetPagedHeadersAsync(null, 0, 10)).ShouldBeEmpty();
                (await _runRepository.FindWithDetailsAsync(graph.RunId)).ShouldBeNull();
            }
        });
    }

    [Fact]
    public async Task Completed_Reference_Run_Should_Require_Same_Definition_Tenant_And_Older_Order()
    {
        var engineId = await GetLookupIdAsync(_databaseEngineRepository, DatabaseEngineCodes.PostgreSql);
        var comparisonTypeId = await GetLookupIdAsync(_comparisonTypeRepository, ComparisonTypeCodes.SchemaOnly);
        var completedStatusId = await GetLookupIdAsync(_runStatusRepository, ComparisonRunStatusCodes.Completed);
        var ownerTenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        var reference = await CreateRunGraphAsync(ownerTenantId, engineId, comparisonTypeId, completedStatusId);
        var currentRunId = await CreateAdditionalRunAsync(
            ownerTenantId, reference.DefinitionId, reference.SourceConnectionId,
            reference.TargetConnectionId, comparisonTypeId, completedStatusId);
        var unrelatedRunId = await CreateUnrelatedDefinitionRunAsync(
            ownerTenantId, reference.SourceConnectionId, reference.TargetConnectionId,
            comparisonTypeId, completedStatusId);

        await WithUnitOfWorkAsync(async () =>
        {
            using (_currentTenant.Change(ownerTenantId))
            {
                (await _runRepository.IsValidCompletedReferenceRunAsync(currentRunId, reference.RunId))
                    .ShouldBeTrue();
                (await _runRepository.IsValidCompletedReferenceRunAsync(unrelatedRunId, reference.RunId))
                    .ShouldBeFalse();
                (await _runRepository.IsValidCompletedReferenceRunAsync(reference.RunId, currentRunId))
                    .ShouldBeFalse();
            }

            using (_currentTenant.Change(otherTenantId))
            {
                (await _runRepository.IsValidCompletedReferenceRunAsync(currentRunId, reference.RunId))
                    .ShouldBeFalse();
            }
        });
    }

    private async Task<Guid> GetLookupIdAsync<TEntity>(IRepository<TEntity, Guid> repository, string code)
        where TEntity : LookupEntity
    {
        return await WithUnitOfWorkAsync(async () =>
            (await repository.GetListAsync(x => x.Code == code)).Single().Id);
    }

    private async Task<(Guid RunId, Guid SourceConnectionId, Guid TargetConnectionId, Guid DefinitionId)> CreateRunGraphAsync(
        Guid tenantId,
        Guid engineId,
        Guid comparisonTypeId,
        Guid completedStatusId)
    {
        return await WithUnitOfWorkAsync(async () =>
        {
            using (_currentTenant.Change(tenantId))
            {
                var source = new DatabaseConnection(_guidGenerator.Create())
                {
                    EngineId = engineId,
                    Name = "history-source",
                    Host = "localhost",
                    Port = 5432,
                    DatabaseName = "history_source",
                    VaultSecretPath = "test/history-source",
                    IsActive = true
                };
                var target = new DatabaseConnection(_guidGenerator.Create())
                {
                    EngineId = engineId,
                    Name = "history-target",
                    Host = "localhost",
                    Port = 5432,
                    DatabaseName = "history_target",
                    VaultSecretPath = "test/history-target",
                    IsActive = true
                };
                await _connectionRepository.InsertManyAsync(new[] { source, target }, autoSave: true);

                var definition = new ComparisonDefinition(_guidGenerator.Create())
                {
                    Name = "history-definition",
                    SourceConnectionId = source.Id,
                    TargetConnectionId = target.Id,
                    ComparisonTypeId = comparisonTypeId,
                    IsActive = true
                };
                await _definitionRepository.InsertAsync(definition, autoSave: true);

                var run = new ComparisonRun(_guidGenerator.Create())
                {
                    ComparisonDefinitionId = definition.Id,
                    SourceConnectionId = source.Id,
                    TargetConnectionId = target.Id,
                    ComparisonTypeId = comparisonTypeId,
                    StatusId = completedStatusId,
                    SchemaDifferenceCount = 1
                };
                await _runRepository.InsertAsync(run, autoSave: true);

                return (run.Id, source.Id, target.Id, definition.Id);
            }
        });
    }

    private async Task PassivateReferencesAsync(
        Guid tenantId,
        Guid sourceConnectionId,
        Guid targetConnectionId,
        Guid definitionId)
    {
        await WithUnitOfWorkAsync(async () =>
        {
            using (_currentTenant.Change(tenantId))
            {
                var source = await _connectionRepository.GetAsync(sourceConnectionId);
                var target = await _connectionRepository.GetAsync(targetConnectionId);
                var definition = await _definitionRepository.GetAsync(definitionId);

                source.IsActive = false;
                target.IsActive = false;
                definition.IsActive = false;

                await _connectionRepository.UpdateManyAsync(new[] { source, target }, autoSave: true);
                await _definitionRepository.UpdateAsync(definition, autoSave: true);
            }
        });
    }

    // islevi: Run durum olayindaki New bulgu sayisinin dayandigi onceki-kosu cozumunu pinler.
    // sistemdeki gorevi: Ilk kosuda null, ayni definition icindeki sonraki kosuda referans kumesi donmesini kalici guvenceye alir.
    [Fact]
    public async Task Previous_Completed_Fingerprints_Should_Be_Null_For_A_First_Run_And_Resolved_For_A_Later_One()
    {
        var engineId = await GetLookupIdAsync(_databaseEngineRepository, DatabaseEngineCodes.PostgreSql);
        var comparisonTypeId = await GetLookupIdAsync(_comparisonTypeRepository, ComparisonTypeCodes.SchemaOnly);
        var completedStatusId = await GetLookupIdAsync(_runStatusRepository, ComparisonRunStatusCodes.Completed);
        var tenantId = Guid.NewGuid();
        var graph = await CreateRunGraphAsync(tenantId, engineId, comparisonTypeId, completedStatusId);
        var laterRunId = await CreateAdditionalRunAsync(
            tenantId,
            graph.DefinitionId,
            graph.SourceConnectionId,
            graph.TargetConnectionId,
            comparisonTypeId,
            completedStatusId);

        await WithUnitOfWorkAsync(async () =>
        {
            using (_currentTenant.Change(tenantId))
            {
                var forFirstRun = await _runRepository.FindPreviousCompletedFindingFingerprintsAsync(graph.RunId);
                var forLaterRun = await _runRepository.FindPreviousCompletedFindingFingerprintsAsync(laterRunId);

                forFirstRun.ShouldBeNull();
                forLaterRun.ShouldNotBeNull();
            }
        });
    }

    // islevi: Mevcut definition icin referanstan sonra olusan ikinci run'i kurar.
    private async Task<Guid> CreateAdditionalRunAsync(
        Guid tenantId,
        Guid definitionId,
        Guid sourceConnectionId,
        Guid targetConnectionId,
        Guid comparisonTypeId,
        Guid completedStatusId)
    {
        return await WithUnitOfWorkAsync(async () =>
        {
            using (_currentTenant.Change(tenantId))
            {
                var run = new ComparisonRun(_guidGenerator.Create())
                {
                    ComparisonDefinitionId = definitionId,
                    SourceConnectionId = sourceConnectionId,
                    TargetConnectionId = targetConnectionId,
                    ComparisonTypeId = comparisonTypeId,
                    StatusId = completedStatusId
                };
                await _runRepository.InsertAsync(run, autoSave: true);
                return run.Id;
            }
        });
    }

    // islevi: Ayni tenant ve baglantilarda farkli definition'a bagli run kurarak relation reddini kanitlar.
    private async Task<Guid> CreateUnrelatedDefinitionRunAsync(
        Guid tenantId,
        Guid sourceConnectionId,
        Guid targetConnectionId,
        Guid comparisonTypeId,
        Guid completedStatusId)
    {
        return await WithUnitOfWorkAsync(async () =>
        {
            using (_currentTenant.Change(tenantId))
            {
                var definition = new ComparisonDefinition(_guidGenerator.Create())
                {
                    Name = $"unrelated-{_guidGenerator.Create():N}",
                    SourceConnectionId = sourceConnectionId,
                    TargetConnectionId = targetConnectionId,
                    ComparisonTypeId = comparisonTypeId,
                    IsActive = true
                };
                await _definitionRepository.InsertAsync(definition, autoSave: true);
                var run = new ComparisonRun(_guidGenerator.Create())
                {
                    ComparisonDefinitionId = definition.Id,
                    SourceConnectionId = sourceConnectionId,
                    TargetConnectionId = targetConnectionId,
                    ComparisonTypeId = comparisonTypeId,
                    StatusId = completedStatusId
                };
                await _runRepository.InsertAsync(run, autoSave: true);
                return run.Id;
            }
        });
    }
}
