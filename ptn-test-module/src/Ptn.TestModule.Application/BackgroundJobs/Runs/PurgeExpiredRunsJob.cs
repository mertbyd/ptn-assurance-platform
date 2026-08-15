using System.Linq;
using System.Threading.Tasks;
using Ptn.TestModule.BackgroundJobs.Shared;
using Ptn.TestModule.Interface.Runs;
using Ptn.TestModule.Managers.Runs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;
using Volo.Abp.Uow;

namespace Ptn.TestModule.BackgroundJobs.Runs;

// islevi: Suresi dolan HAR blob'larini ve tamamlanmis kosum satirlarini setting-driven dilimler halinde temizler.
// sistemdeki gorevi: ABP job, tenant ve UOW sinirlarini korurken retention business kararlarini Manager'a devreder.
public class PurgeExpiredRunsJob
    : TestModuleTenantBackgroundJob<PurgeExpiredRunsArgs>, ITransientDependency
{
    private readonly RunRetentionManager _retentionManager;
    private readonly IHarArtifactStore _harArtifactStore;

    public PurgeExpiredRunsJob(
        RunRetentionManager retentionManager,
        IHarArtifactStore harArtifactStore,
        ICurrentTenant currentTenant,
        IUnitOfWorkManager unitOfWorkManager,
        ICancellationTokenProvider cancellationTokenProvider)
        : base(currentTenant, unitOfWorkManager, cancellationTokenProvider)
    {
        _retentionManager = retentionManager;
        _harArtifactStore = harArtifactStore;
    }

    /// <summary>Bir HAR ve bir kosum batch'ini retry-safe sirayla temizler.</summary>
    protected override async Task ExecuteInTenantAsync(PurgeExpiredRunsArgs args)
    {
        var plan = await RunInUnitOfWorkAsync(() => _retentionManager.CreatePlanAsync(JobCancellationToken));
        var artifacts = await RunInUnitOfWorkAsync(
            () => _retentionManager.GetExpiredHarArtifactsAsync(plan, JobCancellationToken));

        await _harArtifactStore.DeleteManyAsync(
            artifacts.Select(entity => entity.HarBlobName).OfType<string>().ToArray(),
            JobCancellationToken);

        await RunInUnitOfWorkAsync(
            () => _retentionManager.ClearHarArtifactNamesAsync(artifacts, JobCancellationToken));

        var runs = await RunInUnitOfWorkAsync(
            () => _retentionManager.GetExpiredRunsAsync(plan, JobCancellationToken));
        await _harArtifactStore.DeleteManyAsync(
            runs.Select(entity => entity.HarBlobName).OfType<string>().ToArray(),
            JobCancellationToken);

        await RunInUnitOfWorkAsync(
            () => _retentionManager.DeleteExpiredRunsAsync(runs, JobCancellationToken));
    }
}
