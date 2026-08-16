using Ptn.DatabaseChecker.BackgroundJobs;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;
using Volo.Abp.Uow;

namespace Ptn.DatabaseChecker.Application.BackgroundJobs.Shared;

// islevi: Tenant-aware Database Checker job'larinin ortak runtime kabugunu tanimlar.
// sistemdeki gorevi: CurrentTenant, cancellation ve requires-new UnitOfWork tekrarini concrete checker job'larindan kaldirir.
public abstract class DatabaseCheckerTenantBackgroundJob<TArgs> : AsyncBackgroundJob<TArgs>
    where TArgs : class, ITenantBackgroundJobArgs
{
    private readonly ICurrentTenant _currentTenant;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly ICancellationTokenProvider _cancellationTokenProvider;

    protected DatabaseCheckerTenantBackgroundJob(
        ICurrentTenant currentTenant,
        IUnitOfWorkManager unitOfWorkManager,
        ICancellationTokenProvider cancellationTokenProvider)
    {
        _currentTenant = currentTenant;
        _unitOfWorkManager = unitOfWorkManager;
        _cancellationTokenProvider = cancellationTokenProvider;
    }

    protected CancellationToken JobCancellationToken => _cancellationTokenProvider.Token;

    public sealed override async Task ExecuteAsync(TArgs args)
    {
        JobCancellationToken.ThrowIfCancellationRequested();
        using (_currentTenant.Change(args.TenantId))
        {
            await ExecuteInTenantAsync(args);
        }
    }

    protected abstract Task ExecuteInTenantAsync(TArgs args);

    protected async Task RunInUnitOfWorkAsync(Func<Task> action)
    {
        using var unitOfWork = _unitOfWorkManager.Begin(requiresNew: true);
        await action();
        await unitOfWork.CompleteAsync();
    }

    protected async Task<TResult> RunInUnitOfWorkAsync<TResult>(Func<Task<TResult>> action)
    {
        using var unitOfWork = _unitOfWorkManager.Begin(requiresNew: true);
        var result = await action();
        await unitOfWork.CompleteAsync();
        return result;
    }
}
