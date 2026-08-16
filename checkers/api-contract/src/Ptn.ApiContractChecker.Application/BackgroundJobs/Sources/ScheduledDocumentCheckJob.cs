using Ptn.ApiContractChecker.Application.BackgroundJobs.Shared;
using Ptn.ApiContractChecker.BackgroundJobs.Runs;
using Ptn.ApiContractChecker.BackgroundJobs.Sources;
using Ptn.ApiContractChecker.Managers.Runs;
using Ptn.ApiContractChecker.Managers.Sources;
using Ptn.ApiContractChecker.Models.Sources;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;
using Volo.Abp.Uow;

namespace Ptn.ApiContractChecker.Application.BackgroundJobs.Sources;

// islevi: Vadesi gelmis bir dokumani ceker, ingest eder ve icerik degistiyse mevcut karsilastirma yolunu tetikler.
// sistemdeki gorevi: Zamanlanmis izlemenin tek is birimidir; worker'i cekim ve diff yukunden, ADR-0006'yi uzun UOW'dan korur.
public class ScheduledDocumentCheckJob
    : ApiContractCheckerTenantBackgroundJob<ScheduledDocumentCheckJobArgs>, ITransientDependency
{
    // Vade ilerletme, cekim ve degisim sinyali kararlarinin tek domain kapisi.
    private readonly ScheduledSpecDocumentCheckManager _checkManager;

    // Pending run yazimini elle tetikleme yoluyla ayni domain kapisindan alir.
    private readonly ContractCheckRunExecutionManager _executionManager;

    // Karsilastirmayi mevcut execution job'ina devreder; bu is burada calismaz.
    private readonly IBackgroundJobManager _backgroundJobManager;

    public ScheduledDocumentCheckJob(
        ScheduledSpecDocumentCheckManager checkManager,
        ContractCheckRunExecutionManager executionManager,
        IBackgroundJobManager backgroundJobManager,
        ICurrentTenant currentTenant,
        IUnitOfWorkManager unitOfWorkManager,
        ICancellationTokenProvider cancellationTokenProvider)
        : base(currentTenant, unitOfWorkManager, cancellationTokenProvider)
    {
        _checkManager = checkManager;
        _executionManager = executionManager;
        _backgroundJobManager = backgroundJobManager;
    }

    // Vadeyi ilerletir, UOW disinda ceker, dedup yeni satir actiysa karsilastirmayi kuyruklar.
    protected override async Task ExecuteInTenantAsync(ScheduledDocumentCheckJobArgs args)
    {
        // 1) Kisa UOW: baglami kurar ve vadeyi cekimden once yazar; erisilemeyen servis dokumani susturmaz.
        var context = await RunInUnitOfWorkAsync(
            () => _checkManager.BeginAsync(args.SpecSourceId, args.SpecDocumentId));

        // 2) UOW YOK: uzun dis I/O burada, hicbir transaction acik degil (ADR-0006).
        SpecFetchResultModel fetched;
        try
        {
            fetched = await _checkManager.FetchAsync(context);
        }
        catch
        {
            // Cekim sonucu ayri kisa UOW'de kalir; exception ABP job retry sinirina aynen geri verilir.
            await RunInUnitOfWorkAsync(() => _checkManager.RecordUnreachableAsync(context));
            throw;
        }

        // 3) Kisa UOW: dedup karari. Icerik degismediyse cift olusmaz ve calistirma acilmaz.
        var pair = await RunInUnitOfWorkAsync(() => _checkManager.IngestAsync(context, fetched));
        if (pair is null)
        {
            return;
        }

        await TriggerComparisonAsync(pair, args.TenantId);
    }

    // Pending run'i kisa UOW'de yazip mevcut execution job'ini varsayilan kapsamla kuyruga alir (ADR-0005).
    private async Task TriggerComparisonAsync(ScheduledDocumentCheckPairModel pair, Guid? tenantId)
    {
        var run = await RunInUnitOfWorkAsync(
            () => _executionManager.PrepareAsync(pair.BaseSnapshotId, pair.TargetSnapshotId));

        await RunInUnitOfWorkAsync(() => _backgroundJobManager.EnqueueAsync(new ContractCheckExecutionJobArgs
        {
            RunId = run.Id,
            TenantId = tenantId
        }));
    }
}
