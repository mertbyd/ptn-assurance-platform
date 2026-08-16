using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ptn.DatabaseChecker.Entities.Lookups;
using Ptn.DatabaseChecker.Entities.Runs;
using Ptn.DatabaseChecker.Entities.Definitions;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Events.Runs;
using Ptn.DatabaseChecker.ExceptionCodes;
using Ptn.DatabaseChecker.Interface.Connections;
using Ptn.DatabaseChecker.Interface.Definitions;
using Ptn.DatabaseChecker.Interface.Runs;
using Ptn.DatabaseChecker.Models.Runs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Entities;
using Volo.Abp;
using Volo.Abp.EventBus.Local;

namespace Ptn.DatabaseChecker.Managers.Runs;

// islevi: Karsilastirma run kaydinin tarif/baglanti/mod/durum FK kurallarini isletir.
// sistemdeki gorevi: Run baslatma akisini yalnizca var olan referanslarla ilerletecek domain kapisini saglar.
public class ComparisonRunManager : BaseManager<ComparisonRun>
{
    // Opsiyonel tarif FK varligi tipli repository uzerinden dogrulanir.
    private IComparisonDefinitionRepository DefinitionRepository => LazyGetRequiredService<IComparisonDefinitionRepository>();

    // Baglanti FK varliklari tipli repository uzerinden dogrulanir.
    private IDatabaseConnectionRepository ConnectionRepository => LazyGetRequiredService<IDatabaseConnectionRepository>();

    // ComparisonType FK varligi lookup repository uzerinden dogrulanir.
    private IRepository<ComparisonType, Guid> ComparisonTypeRepository => LazyGetRequiredService<IRepository<ComparisonType, Guid>>();

    // Status FK varligi lookup repository uzerinden dogrulanir.
    private IRepository<ComparisonRunStatus, Guid> StatusRepository => LazyGetRequiredService<IRepository<ComparisonRunStatus, Guid>>();

    // Ortak Notifications/Test/MCP adapterlerine tasima bagimsiz durum olayi yayinlar.
    private ILocalEventBus LocalEventBus => LazyGetRequiredService<ILocalEventBus>();

    // Olay ozetindeki New bulgu sayisi icin run'a ozel fingerprint projeksiyonunu kullanir.
    private IComparisonRunRepository RunRepository => (IComparisonRunRepository)Repository;

    public ComparisonRunManager(
        IComparisonRunRepository repository,
        IAbpLazyServiceProvider abpLazyServiceProvider)
        : base(repository, abpLazyServiceProvider)
    {
    }

    // islevi: Yeni run modelindeki opsiyonel tarif, baglanti, mod ve durum referanslarini dogrular.
    public async Task<CreateComparisonRunModel> ValidateCreateAsync(CreateComparisonRunModel model)
    {
        await EnsureReferencesExistAsync(model);
        return model;
    }

    // islevi: Toplu run olusturmada tum FK referanslarini tekil sorgularla dogrular.
    public async Task<List<CreateComparisonRunModel>> ValidateCreateManyAsync(List<CreateComparisonRunModel> models)
    {
        await EnsureDefinitionsExistAsync(models.Select(x => x.ComparisonDefinitionId));
        await EnsureConnectionsExistAsync(models.SelectMany(x => new[] { x.SourceConnectionId, x.TargetConnectionId }));
        await EnsureComparisonTypesExistAsync(models.Select(x => x.ComparisonTypeId));
        await EnsureStatusesExistAsync(models.Select(x => x.StatusId));
        return models;
    }

    // islevi: Pending run'i Running durumuna alir; terminal run'larda duplicate job execution'ini sessizce atlar.
    // sistemdeki gorevi: Background worker retry/restart halinde status gecisini idempotent tutar ve StartedAt'i ilk claim aninda yazar.
    public async Task<bool> StartExecutionAsync(Guid runId)
    {
        var run = await LoadWithDetailsAsync(runId);
        if (run.Status.Code == ComparisonRunStatusCodes.Completed ||
            run.Status.Code == ComparisonRunStatusCodes.Failed)
        {
            return false;
        }

        if (run.Status.Code == ComparisonRunStatusCodes.Running)
        {
            return true;
        }

        if (run.Status.Code != ComparisonRunStatusCodes.Pending)
        {
            throw new BusinessException(ComparisonRunExceptionCodes.InvalidStatusTransition);
        }

        var runningStatus = await ResolveStatusAsync(ComparisonRunStatusCodes.Running);
        run.StatusId = runningStatus.Id;
        run.Status = runningStatus;
        run.StartedAt = Clock.Now;
        run.CompletedAt = null;
        run.ErrorMessage = null;
        await Repository.UpdateAsync(run, autoSave: true);
        await PublishStatusChangedAsync(run, ComparisonRunStatusCodes.Running);
        return true;
    }

    // islevi: Motor sonucunu Running run'a Mapperly delegate'iyle uygular ve Completed olarak saklar.
    // sistemdeki gorevi: Domain status kurali manager'da kalirken Application mapping implementasyonu Domain'e referans sizdirmadan kullanilir.
    public async Task CompleteExecutionAsync(
        Guid runId,
        ComparisonRunExecutionResultModel result,
        Action<ComparisonRunExecutionResultModel, ComparisonRun> mapResult)
    {
        var run = await LoadWithDetailsAsync(runId);
        if (run.Status.Code != ComparisonRunStatusCodes.Running)
        {
            throw new BusinessException(ComparisonRunExceptionCodes.InvalidStatusTransition);
        }

        mapResult(result, run);
        var completedStatus = await ResolveStatusAsync(ComparisonRunStatusCodes.Completed);
        run.StatusId = completedStatus.Id;
        run.Status = completedStatus;
        run.ErrorMessage = null;
        await Repository.UpdateAsync(run, autoSave: true);
        await PublishStatusChangedAsync(run, ComparisonRunStatusCodes.Completed);
    }

    // islevi: Pending/Running run'i guvenli hata koduyla terminal Failed durumuna alir.
    // sistemdeki gorevi: Background execution exception'i HTTP disinda kaybolmaz; ham exception/secret bilgisi run kaydina girmez.
    public async Task FailExecutionAsync(Guid runId, string errorCode)
    {
        var run = await LoadWithDetailsAsync(runId);
        if (run.Status.Code == ComparisonRunStatusCodes.Completed ||
            run.Status.Code == ComparisonRunStatusCodes.Failed)
        {
            return;
        }

        var failedStatus = await ResolveStatusAsync(ComparisonRunStatusCodes.Failed);
        run.StatusId = failedStatus.Id;
        run.Status = failedStatus;
        run.CompletedAt = Clock.Now;
        run.ErrorMessage = errorCode;
        await Repository.UpdateAsync(run, autoSave: true);
        await PublishStatusChangedAsync(run, ComparisonRunStatusCodes.Failed);
    }

    // islevi: Kalici run durum gecisini tenant adresi ve bulgu ozetiyle local event bus'a verir.
    // sistemdeki gorevi: Checker notification tasimasini bilmeden ortak modullerin olayi ek sayfa okumadan tuketmesini saglar.
    private async Task PublishStatusChangedAsync(ComparisonRun run, string statusCode)
    {
        var fingerprints = CollectFingerprints(run);
        var newFindingCount = await CountNewFindingsAsync(run.Id, fingerprints);
        await LocalEventBus.PublishAsync(
            new ComparisonRunStatusChangedEto(
                run.Id,
                run.TenantId,
                statusCode,
                newFindingCount,
                ResolveMaxSeverityCode(run)));
    }

    // islevi: Bulgusuz geciste repository'ye gitmeden sifirlar, aksi halde onceki tamamlanmis kosuda gorulmemisleri sayar.
    private async Task<int> CountNewFindingsAsync(Guid runId, HashSet<string> fingerprints)
    {
        if (fingerprints.Count == 0)
        {
            return 0;
        }

        var previous = await RunRepository.FindPreviousCompletedFindingFingerprintsAsync(runId);
        if (previous is null)
        {
            return fingerprints.Count;
        }

        return fingerprints.Count(fingerprint => !previous.Contains(fingerprint));
    }

    // islevi: Kosudaki bulgulardan en agir siddet kodunu kararli oncelik sirasiyla cozer; bulgu yoksa null doner.
    private static string? ResolveMaxSeverityCode(ComparisonRun run)
    {
        var present = CollectSeverityCodes(run);
        return DifferenceSeverityCodes.Ranked.FirstOrDefault(present.Contains);
    }

    // islevi: Uc owned bulgu ailesinin siddet kodlarini tek ordinal kumede toplar.
    private static HashSet<string> CollectSeverityCodes(ComparisonRun run)
    {
        var result = new HashSet<string>(StringComparer.Ordinal);
        if (run.Findings is null)
        {
            return result;
        }

        result.UnionWith(run.Findings.SchemaDifferences.Select(finding => finding.SeverityCode));
        result.UnionWith(run.Findings.MigrationDifferences.Select(finding => finding.SeverityCode));
        result.UnionWith(run.Findings.DataDifferences.Select(finding => finding.SeverityCode));
        return result;
    }

    // islevi: Uc owned bulgu ailesinin fingerprintlerini tek ordinal kumede toplar; legacy null degerleri disarida birakir.
    private static HashSet<string> CollectFingerprints(ComparisonRun run)
    {
        var result = new HashSet<string>(StringComparer.Ordinal);
        if (run.Findings is null)
        {
            return result;
        }

        AddFingerprints(result, run.Findings.SchemaDifferences.Select(finding => finding.Fingerprint));
        AddFingerprints(result, run.Findings.MigrationDifferences.Select(finding => finding.Fingerprint));
        AddFingerprints(result, run.Findings.DataDifferences.Select(finding => finding.Fingerprint));
        return result;
    }

    // islevi: Null olmayan fingerprintleri hedef kumeye ekler.
    private static void AddFingerprints(HashSet<string> target, IEnumerable<string?> fingerprints)
    {
        foreach (var fingerprint in fingerprints)
        {
            if (fingerprint is not null)
            {
                target.Add(fingerprint);
            }
        }
    }

    // islevi: Run'i status navigation'i ve owned sonucuyla yukler.
    private async Task<ComparisonRun> LoadWithDetailsAsync(Guid runId)
    {
        var run = await ((IComparisonRunRepository)Repository).FindWithDetailsAsync(runId);
        if (run is null)
        {
            throw new BusinessException(GeneralExceptionCodes.NotFound);
        }

        return run;
    }

    // islevi: Kararli status kodunu lookup entity'sine cevirir.
    private async Task<ComparisonRunStatus> ResolveStatusAsync(string statusCode)
    {
        var status = await StatusRepository.FirstOrDefaultAsync(candidate => candidate.Code == statusCode);
        if (status is null)
        {
            throw new BusinessException(GeneralExceptionCodes.InvalidOperation);
        }

        return status;
    }

    // islevi: Tek run modelinin tum referanslarini dogrular.
    private async Task EnsureReferencesExistAsync(CreateComparisonRunModel model)
    {
        if (model.ComparisonDefinitionId.HasValue)
        {
            var definition = await DefinitionRepository.FindWithDetailsAsync(model.ComparisonDefinitionId.Value);
            if (definition is null)
            {
                throw new EntityNotFoundException(
                    typeof(ComparisonDefinition),
                    model.ComparisonDefinitionId.Value);
            }
        }
        await EnsureConnectionsExistAsync(new[] { model.SourceConnectionId, model.TargetConnectionId });
        await EnsureExistsInAsync(ComparisonTypeRepository, model.ComparisonTypeId);
        await EnsureExistsInAsync(StatusRepository, model.StatusId);
    }

    // islevi: Opsiyonel tarif referanslarini tek sorguyla dogrular.
    private async Task EnsureDefinitionsExistAsync(IEnumerable<Guid?> comparisonDefinitionIds)
    {
        var ids = comparisonDefinitionIds
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();
        if (ids.Count == 0)
        {
            return;
        }

        var found = await DefinitionRepository.GetAccessibleByIdsAsync(ids);
        EnsureAllIdsFound<ComparisonDefinition>(ids, found.Select(x => x.Id));
    }

    // islevi: Baglanti referanslarini tek sorguyla dogrular.
    private async Task EnsureConnectionsExistAsync(IEnumerable<Guid> connectionIds)
    {
        await EnsureAllExistInAsync(ConnectionRepository, connectionIds);
    }

    // islevi: ComparisonType lookup referanslarini tek sorguyla dogrular.
    private async Task EnsureComparisonTypesExistAsync(IEnumerable<Guid> comparisonTypeIds)
    {
        await EnsureAllExistInAsync(ComparisonTypeRepository, comparisonTypeIds);
    }

    // islevi: Run status lookup referanslarini tek sorguyla dogrular.
    private async Task EnsureStatusesExistAsync(IEnumerable<Guid> statusIds)
    {
        await EnsureAllExistInAsync(StatusRepository, statusIds);
    }
}
