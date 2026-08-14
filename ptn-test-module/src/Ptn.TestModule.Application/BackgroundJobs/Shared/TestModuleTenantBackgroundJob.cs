using System;
using System.Threading;
using System.Threading.Tasks;
using Ptn.TestModule.BackgroundJobs;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;
using Volo.Abp.Uow;

namespace Ptn.TestModule.BackgroundJobs.Shared;

// islevi: Tenant-aware Test Module background job'larinin ortak runtime kabugunu tanimlar.
// sistemdeki gorevi: CurrentTenant, cancellation ve requires-new UnitOfWork tekrarini concrete job'lardan kaldirir; ABP kuyruk ve retry altyapisini kullanmaya devam eder.
public abstract class TestModuleTenantBackgroundJob<TArgs> : AsyncBackgroundJob<TArgs>
    where TArgs : class, ITestModuleTenantBackgroundJobArgs
{
    // Job calisirken etkin tenant baglamini acar.
    private readonly ICurrentTenant _currentTenant;

    // Concrete job adimlarinin kalici islemleri icin yeni UOW baslatir.
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    // ABP worker shutdown ve cancellation sinyalini concrete job'lara tasir.
    private readonly ICancellationTokenProvider _cancellationTokenProvider;

    protected TestModuleTenantBackgroundJob(
        ICurrentTenant currentTenant,
        IUnitOfWorkManager unitOfWorkManager,
        ICancellationTokenProvider cancellationTokenProvider)
    {
        _currentTenant = currentTenant;
        _unitOfWorkManager = unitOfWorkManager;
        _cancellationTokenProvider = cancellationTokenProvider;
    }

    // Concrete job'larin iptal ve timeout kararlarinda kullandigi ABP token'idir.
    protected CancellationToken JobCancellationToken => _cancellationTokenProvider.Token;

    // islevi: Job payload'i icin tenant context acip concrete business akisina devreder.
    // sistemdeki gorevi: Her tenant job'inda CurrentTenant.Change ve ilk cancellation kontrolunu tek noktada uygular.
    public sealed override async Task ExecuteAsync(TArgs args)
    {
        JobCancellationToken.ThrowIfCancellationRequested();
        using (_currentTenant.Change(args.TenantId))
        {
            await ExecuteInTenantAsync(args);
        }
    }

    // islevi: Tenant context acildiktan sonra calisacak concrete job adimini tanimlar.
    // sistemdeki gorevi: Concrete job'larin yalnizca kendi use-case akisina odaklanmasini saglar.
    protected abstract Task ExecuteInTenantAsync(TArgs args);

    // islevi: Sonuc dondurmeyen job adimini requires-new UOW ile calistirir.
    // sistemdeki gorevi: Background worker scope'unda kalici write'larin tamamlanmasini ortaklastirir.
    protected async Task RunInUnitOfWorkAsync(Func<Task> action)
    {
        using var unitOfWork = _unitOfWorkManager.Begin(requiresNew: true);
        await action();
        await unitOfWork.CompleteAsync();
    }

    // islevi: Sonuc donduren job adimini requires-new UOW ile calistirir.
    // sistemdeki gorevi: Claim ve terminal yazim gibi repository sonucuna ihtiyac duyan akislarin UOW tekrarini kaldirir.
    protected async Task<TResult> RunInUnitOfWorkAsync<TResult>(Func<Task<TResult>> action)
    {
        using var unitOfWork = _unitOfWorkManager.Begin(requiresNew: true);
        var result = await action();
        await unitOfWork.CompleteAsync();
        return result;
    }
}
