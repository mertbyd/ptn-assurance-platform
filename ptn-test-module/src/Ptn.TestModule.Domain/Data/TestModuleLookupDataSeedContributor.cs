using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nexum.Abp.Foundation.Lookups;
using Ptn.TestModule.Constants.Runs.Lookups;
using Ptn.TestModule.Entities.Lookups;
using Ptn.TestModule.Interface.Lookups;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Uow;

namespace Ptn.TestModule.Data;

// islevi: Bes lookup tablosunun kararli kod kumesini idempotent olarak uretir.
// sistemdeki gorevi: Sema kurulduktan sonra FK'lerin baglanabilecegi referans verisini hazirlar; tekrar kosum satir cogaltmaz (ADR-0016 §F).
public class TestModuleLookupDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    // Build'i kiran hukumler; politika kodda dallanmaz, breaks_build kolonuna yazilir (ADR-0016 §F).
    private static readonly IReadOnlyCollection<string> BuildBreakingOutcomeCodes =
        [TestOutcomeStatusCodes.Failed, TestOutcomeStatusCodes.Broken];

    private readonly ITestRunStatusRepository _runStatusRepository;
    private readonly ITestOutcomeStatusRepository _outcomeStatusRepository;
    private readonly ITestFailureCategoryRepository _failureCategoryRepository;
    private readonly ITestTriggerKindRepository _triggerKindRepository;
    private readonly ITestScenarioStateRepository _scenarioStateRepository;
    private readonly IGuidGenerator _guidGenerator;

    public TestModuleLookupDataSeedContributor(
        ITestRunStatusRepository runStatusRepository,
        ITestOutcomeStatusRepository outcomeStatusRepository,
        ITestFailureCategoryRepository failureCategoryRepository,
        ITestTriggerKindRepository triggerKindRepository,
        ITestScenarioStateRepository scenarioStateRepository,
        IGuidGenerator guidGenerator)
    {
        _runStatusRepository = runStatusRepository;
        _outcomeStatusRepository = outcomeStatusRepository;
        _failureCategoryRepository = failureCategoryRepository;
        _triggerKindRepository = triggerKindRepository;
        _scenarioStateRepository = scenarioStateRepository;
        _guidGenerator = guidGenerator;
    }

    [UnitOfWork]
    public async Task SeedAsync(DataSeedContext context)
    {
        await SeedLookupAsync(_runStatusRepository, TestRunStatusCodes.All,
            (id, code) => new TestRunStatus(id, code, code));

        await SeedLookupAsync(_outcomeStatusRepository, TestOutcomeStatusCodes.All,
            (id, code) => new TestOutcomeStatus(id, code, code, BuildBreakingOutcomeCodes.Contains(code)));

        await SeedLookupAsync(_failureCategoryRepository, TestFailureCategoryCodes.All,
            (id, code) => new TestFailureCategory(id, code, code));

        await SeedLookupAsync(_triggerKindRepository, TestTriggerKindCodes.All,
            (id, code) => new TestTriggerKind(id, code, code));

        await SeedLookupAsync(_scenarioStateRepository, TestScenarioStateCodes.All,
            (id, code) => new TestScenarioState(id, code, code));
    }

    // Eksik kodlari tek sorgu ve tek InsertMany ile tamamlar; mevcut satirlar ellenmez, boylece yonetici duzenlemesi korunur ve tekrar kosum satir cogaltmaz.
    private async Task SeedLookupAsync<TEntity>(
        ILookupRepository<TEntity, Guid> repository,
        IReadOnlyCollection<string> desiredCodes,
        Func<Guid, string, TEntity> createEntity)
        where TEntity : LookupEntity<Guid>
    {
        var existingCodes = (await repository.GetListAsync())
            .Select(row => row.Code)
            .ToHashSet();
        var rowsToInsert = desiredCodes
            .Where(code => !existingCodes.Contains(code))
            .Select(code => createEntity(_guidGenerator.Create(), code))
            .ToList();
        if (rowsToInsert.Count > 0)
        {
            await repository.InsertManyAsync(rowsToInsert, autoSave: true);
        }
    }
}
