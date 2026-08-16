using Ptn.ApiContractChecker.Constants;
using Ptn.ApiContractChecker.Constants.Differences.Lookups;
using Ptn.ApiContractChecker.Constants.Runs;
using Ptn.ApiContractChecker.Constants.Runs.Lookups;
using Ptn.ApiContractChecker.Entities.Lookups;
using Ptn.ApiContractChecker.Entities.Runs;
using Ptn.ApiContractChecker.Entities.Snapshots;
using Ptn.ApiContractChecker.ExceptionCodes;
using Ptn.ApiContractChecker.ExceptionCodes.Runs;
using Ptn.ApiContractChecker.Events.Runs;
using Ptn.ApiContractChecker.Interface.Runs;
using Ptn.ApiContractChecker.Managers.Shared;
using Ptn.ApiContractChecker.Models.Runs;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Local;

namespace Ptn.ApiContractChecker.Managers.Runs;

// islevi: ContractCheckRun kurulum, tenant-kapsamli varlik, idempotent claim, terminal gecis ve stale recovery kurallarini isletir.
// sistemdeki gorevi: Ince run entity'sinin claim, terminal gecis, recovery ve tek gecisli sayac davranislarinin tek sahibidir.
public class ContractCheckRunManager : BaseManager<ContractCheckRun>
{
    // Run'a ozel hafif ve detayli sorgulari tek persistence kontratindan kullanir.
    private IContractCheckRunRepository RunRepository => (IContractCheckRunRepository)Repository;

    // Snapshot FK'larini aktif tenant filtresi altinda dogrular.
    private IRepository<SpecSnapshot, Guid> SnapshotRepository
        => LazyGetRequiredService<IRepository<SpecSnapshot, Guid>>();

    // Kararli status kodlarini global lookup kimliklerine cozer.
    private IRepository<CheckRunStatus, Guid> StatusRepository
        => LazyGetRequiredService<IRepository<CheckRunStatus, Guid>>();

    // Kalici durum gecislerini mevcut UOW tamamlandiginda application handler'larina yayimlar.
    private ILocalEventBus LocalEventBus => LazyGetRequiredService<ILocalEventBus>();

    // Olay ozetindeki New bulgu sayisini mevcut degisim siniflandirmasindan alir.
    private FindingChangeStateManager ChangeStateManager
        => LazyGetRequiredService<FindingChangeStateManager>();

    public ContractCheckRunManager(
        IContractCheckRunRepository repository,
        IAbpLazyServiceProvider provider)
        : base(repository, provider)
    {
    }

    // Iki tenant-gorunur snapshot ve Pending status ile yeni run aggregate'i kurar.
    public async Task<ContractCheckRun> CreateAsync(
        Guid id,
        Guid baseSnapshotId,
        Guid targetSnapshotId,
        Guid? tenantId)
    {
        EnsureNotEmpty(id, nameof(id));
        EnsureNotEmpty(baseSnapshotId, nameof(baseSnapshotId));
        EnsureNotEmpty(targetSnapshotId, nameof(targetSnapshotId));
        await EnsureSnapshotsExistAsync(baseSnapshotId, targetSnapshotId);
        var pendingStatus = await ResolveStatusAsync(CheckRunStatusCodes.Pending);
        EnsureNotEmpty(pendingStatus.Id, nameof(pendingStatus.Id));
        return new ContractCheckRun(id, baseSnapshotId, targetSnapshotId, pendingStatus.Id, tenantId);
    }

    // Pending run'i manager'in tek gecis davranisiyla Running durumuna alir.
    public async Task<bool> StartAsync(Guid id)
    {
        var run = await GetRequiredEntityAsync(id);
        if (run.CompletedAt.HasValue)
        {
            return false;
        }

        if (run.StartedAt.HasValue)
        {
            return true;
        }

        var runningStatus = await ResolveStatusAsync(CheckRunStatusCodes.Running);
        Start(run, runningStatus.Id, Clock.Now);
        await Repository.UpdateAsync(run, autoSave: true);
        await AddStatusChangedEvent(run, CheckRunStatusCodes.Running);
        return true;
    }

    // Running run'i manager'in hesapladigi sayaclarla Completed durumuna alir; zaten terminal ise yazmaz.
    public async Task<bool> CompleteAsync(Guid id, ContractCheckFindings findings)
    {
        var run = await GetRequiredEntityAsync(id);
        var completedStatus = await ResolveStatusAsync(CheckRunStatusCodes.Completed);
        if (!Complete(run, completedStatus.Id, Clock.Now, findings))
        {
            return false;
        }

        await Repository.UpdateAsync(run, autoSave: true);
        await AddStatusChangedEvent(run, CheckRunStatusCodes.Completed);
        return true;
    }

    // Running run'i manager'in hata invariantiyla Failed durumuna alir.
    public async Task FailAsync(Guid id, string errorMessage)
    {
        var run = await GetRequiredEntityAsync(id);
        var failedStatus = await ResolveStatusAsync(CheckRunStatusCodes.Failed);
        if (Fail(run, failedStatus.Id, Clock.Now, errorMessage))
        {
            await Repository.UpdateAsync(run, autoSave: true);
            await AddStatusChangedEvent(run, CheckRunStatusCodes.Failed);
        }
    }

    // Running run'i korunabilen bulgular ve hata sebebiyle Partial durumuna alir.
    public async Task CompletePartiallyAsync(
        Guid id,
        ContractCheckFindings findings,
        string errorMessage)
    {
        var run = await GetRequiredEntityAsync(id);
        var partialStatus = await ResolveStatusAsync(CheckRunStatusCodes.Partial);
        if (CompletePartially(run, partialStatus.Id, Clock.Now, findings, errorMessage))
        {
            await Repository.UpdateAsync(run, autoSave: true);
            await AddStatusChangedEvent(run, CheckRunStatusCodes.Partial);
        }
    }

    // Pending run'i tek seferde Running durumuna gecirir.
    public void Start(ContractCheckRun run, Guid runningStatusId, DateTime startedAt)
    {
        if (run.StartedAt.HasValue || IsTerminal(run))
        {
            throw new BusinessException(ContractCheckRunExceptionCodes.InvalidStatusTransition);
        }

        EnsureNotEmpty(runningStatusId, nameof(runningStatusId));
        run.CheckRunStatusId = runningStatusId;
        run.StartedAt = startedAt;
    }

    // Basarili run'i bulgulari ve sayaclariyla idempotent olarak tamamlar.
    public bool Complete(
        ContractCheckRun run,
        Guid completedStatusId,
        DateTime completedAt,
        ContractCheckFindings findings)
    {
        return CompleteTerminal(
            run,
            completedStatusId,
            completedAt,
            findings,
            null,
            requiresError: false);
    }

    // Basarisiz run'i sebebi ve bos bulgu govdesiyle idempotent olarak tamamlar.
    public bool Fail(
        ContractCheckRun run,
        Guid failedStatusId,
        DateTime completedAt,
        string errorMessage)
    {
        return CompleteTerminal(
            run,
            failedStatusId,
            completedAt,
            ContractCheckFindings.Empty(),
            errorMessage,
            requiresError: true);
    }

    // Kismi run'i korunabilen bulgular ve hata sebebiyle idempotent olarak tamamlar.
    public bool CompletePartially(
        ContractCheckRun run,
        Guid partialStatusId,
        DateTime completedAt,
        ContractCheckFindings findings,
        string errorMessage)
    {
        return CompleteTerminal(
            run,
            partialStatusId,
            completedAt,
            findings,
            errorMessage,
            requiresError: true);
    }

    // Esigi asmis Running run'lari tek status cozumu ve toplu yazimla Failed terminal durumuna getirir.
    public async Task<int> RecoverStaleRunningAsync(DateTime startedBefore)
    {
        var runningStatus = await ResolveStatusAsync(CheckRunStatusCodes.Running);
        var failedStatus = await ResolveStatusAsync(CheckRunStatusCodes.Failed);
        var staleRuns = await RunRepository.GetStaleRunningAsync(runningStatus.Id, startedBefore);

        foreach (var run in staleRuns)
        {
            Fail(run, failedStatus.Id, Clock.Now, ContractCheckRunExceptionCodes.StaleRunningRecovered);
        }

        if (staleRuns.Count > 0)
        {
            await Repository.UpdateManyAsync(staleRuns, autoSave: true);
            foreach (var run in staleRuns)
            {
                await AddStatusChangedEvent(run, CheckRunStatusCodes.Failed);
            }
        }

        return staleRuns.Count;
    }

    // Hafif status yoluna tenant-gorunur run basligini zorunlu olarak getirir.
    public async Task<ContractCheckRunHeaderModel> GetRequiredHeaderAsync(Guid id)
    {
        return await RunRepository.FindHeaderAsync(id)
               ?? throw new BusinessException(GeneralExceptionCodes.NotFound);
    }

    // Detay ve rapor yollarina tenant-gorunur run govdesini zorunlu olarak getirir.
    public async Task<ContractCheckRunDetailModel> GetRequiredDetailAsync(Guid id)
    {
        return await RunRepository.FindDetailAsync(id)
               ?? throw new BusinessException(GeneralExceptionCodes.NotFound);
    }

    // Durum gecisi hedefi aggregate'i tenant ve host gorunurlugu altinda zorunlu yukler.
    private async Task<ContractCheckRun> GetRequiredEntityAsync(Guid id)
    {
        return await RunRepository.FindEntityAsync(id)
               ?? throw new BusinessException(GeneralExceptionCodes.NotFound);
    }

    // Kararli status kodunu aktif global lookup satirina cevirir.
    private async Task<CheckRunStatus> ResolveStatusAsync(string statusCode)
    {
        return await StatusRepository.FirstOrDefaultAsync(status => status.Code == statusCode)
               ?? throw new BusinessException(GeneralExceptionCodes.InvalidOperation);
    }

    // Iki snapshot kimliginin aktif tenant filtresi altinda eksiksiz gorundugunu kararli run hatasiyla dogrular.
    private async Task EnsureSnapshotsExistAsync(Guid baseSnapshotId, Guid targetSnapshotId)
    {
        var ids = new HashSet<Guid> { baseSnapshotId, targetSnapshotId };
        var snapshots = await SnapshotRepository.GetListAsync(snapshot => ids.Contains(snapshot.Id));
        if (snapshots.Count != ids.Count)
        {
            throw new BusinessException(ContractCheckRunExceptionCodes.SnapshotNotFound);
        }
    }

    // Kalici durum gecisini tenant adresi ve bulgu ozetiyle local event bus'a verir.
    private async Task AddStatusChangedEvent(ContractCheckRun run, string statusCode)
    {
        var newFindingCount = await CountNewFindingsAsync(run);
        await LocalEventBus.PublishAsync(
            new ContractCheckRunStatusChangedEto(
                run.Id,
                run.TenantId,
                statusCode,
                newFindingCount,
                ResolveMaxSeverityCode(run)));
    }

    // Bulgusuz gecisleri repository'ye gitmeden sifirlar, bulgulu geciste onceki tamamlanmis kosuya gore New sayar.
    private async Task<int> CountNewFindingsAsync(ContractCheckRun run)
    {
        if (run.Findings.Items.Count == 0)
        {
            return 0;
        }

        var classification = await ChangeStateManager.ClassifyAsync(run.Id);
        return classification.NewFingerprints.Count;
    }

    // Terminal geciste hesaplanmis sayaclardan en agir siddet kodunu cozer; bulgu yoksa null doner.
    private static string? ResolveMaxSeverityCode(ContractCheckRun run)
    {
        if (run.BreakingCount > 0)
        {
            return DifferenceSeverityCodes.Breaking;
        }

        if (run.NonBreakingCount > 0)
        {
            return DifferenceSeverityCodes.NonBreaking;
        }

        return run.DocsOnlyCount > 0 ? DifferenceSeverityCodes.DocsOnly : null;
    }
    // Terminal durumda yeniden teslim edilen completion komutunun no-op olmasini saglar.
    private static bool CompleteTerminal(
        ContractCheckRun run,
        Guid terminalStatusId,
        DateTime completedAt,
        ContractCheckFindings? findings,
        string? errorMessage,
        bool requiresError)
    {
        if (IsTerminal(run))
        {
            return false;
        }

        if (!run.StartedAt.HasValue)
        {
            throw new BusinessException(ContractCheckRunExceptionCodes.InvalidStatusTransition);
        }

        if (completedAt < run.StartedAt.Value)
        {
            throw new ArgumentOutOfRangeException(nameof(completedAt));
        }

        EnsureNotEmpty(terminalStatusId, nameof(terminalStatusId));
        if (findings is null)
        {
            throw new BusinessException(ContractCheckRunExceptionCodes.FindingsRequired)
                .WithData(BusinessExceptionDataKeys.Field, nameof(findings));
        }

        var normalizedError = NormalizeError(errorMessage);
        if (requiresError && normalizedError is null)
        {
            throw new BusinessException(ContractCheckRunExceptionCodes.FailureReasonRequired);
        }

        run.Findings = findings;
        run.CheckRunStatusId = terminalStatusId;
        run.CompletedAt = completedAt;
        run.ErrorMessage = normalizedError;
        RecalculateCounts(run);
        return true;
    }

    // Terminal tekrar teslimini yalniz kalici tamamlanma kanitindan belirler.
    private static bool IsTerminal(ContractCheckRun run)
    {
        return run.CompletedAt.HasValue;
    }

    // Owned bulgu listesinden uc denormalize sayaci tek geciste hesaplar.
    private static void RecalculateCounts(ContractCheckRun run)
    {
        run.BreakingCount = 0;
        run.NonBreakingCount = 0;
        run.DocsOnlyCount = 0;

        foreach (var finding in run.Findings.Items)
        {
            switch (finding.SeverityCode)
            {
                case DifferenceSeverityCodes.Breaking:
                    run.BreakingCount++;
                    break;
                case DifferenceSeverityCodes.NonBreaking:
                    run.NonBreakingCount++;
                    break;
                case DifferenceSeverityCodes.DocsOnly:
                    run.DocsOnlyCount++;
                    break;
            }
        }
    }

    // Terminal hata metnini trimler, bos degeri null'a indirger ve kolon sinirini korur.
    private static string? NormalizeError(string? value)
    {
        var normalized = EntityCanonicalizer.NormalizeOptional(value);
        return normalized == null || normalized.Length <= ContractCheckRunConsts.MaxErrorMessageLength
            ? normalized
            : throw new ArgumentOutOfRangeException(nameof(value));
    }

    // Zorunlu FK kimliginin bos Guid olmasini programci hatasi olarak reddeder.
    private static void EnsureNotEmpty(Guid value, string parameterName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(null, parameterName);
        }
    }
}
