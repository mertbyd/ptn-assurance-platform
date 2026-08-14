using System.Threading.Tasks;
using Ptn.TestModule.BackgroundJobs.Shared;
using Ptn.TestModule.Managers.Runs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;
using Volo.Abp.Uow;

namespace Ptn.TestModule.BackgroundJobs.Runs;

// islevi: Motor coktugunde Running'de asili kalan kosum satirlarini esik penceresine gore toparlar.
// sistemdeki gorevi: KBP-93'te yazilan RecoverStaleRunningAsync kapisini ilk kez fiilen cagiran dayanikli supurucudur.
public class RecoverStaleRunsJob
    : TestModuleTenantBackgroundJob<RecoverStaleRunsArgs>, ITransientDependency
{
    // Asili kosum kurtarmasinin tek domain kapisidir.
    private readonly TestRunExecutionManager _executionManager;

    public RecoverStaleRunsJob(
        TestRunExecutionManager executionManager,
        ICurrentTenant currentTenant,
        IUnitOfWorkManager unitOfWorkManager,
        ICancellationTokenProvider cancellationTokenProvider)
        : base(currentTenant, unitOfWorkManager, cancellationTokenProvider)
    {
        _executionManager = executionManager;
    }

    // Kurtarma taramasini tek requires-new UoW icinde calistirir.
    protected override Task ExecuteInTenantAsync(RecoverStaleRunsArgs args)
    {
        return RunInUnitOfWorkAsync(
            () => _executionManager.RecoverStaleAsync(args.ThresholdMinutes, JobCancellationToken));
    }
}
