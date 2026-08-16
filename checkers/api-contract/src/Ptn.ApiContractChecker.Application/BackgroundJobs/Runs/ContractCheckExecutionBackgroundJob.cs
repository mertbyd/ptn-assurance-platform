using Ptn.ApiContractChecker.Application.BackgroundJobs.Shared;
using Ptn.ApiContractChecker.Application.Mappers.Runs;
using Ptn.ApiContractChecker.BackgroundJobs.Runs;
using Ptn.ApiContractChecker.ExceptionCodes.Runs;
using Ptn.ApiContractChecker.Managers.Runs;
using Ptn.ApiContractChecker.Models.Runs;
using Volo.Abp;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;
using Volo.Abp.Uow;

namespace Ptn.ApiContractChecker.Application.BackgroundJobs.Runs;

// islevi: Pending contract-check run'ini kuyruktan alip donmus snapshot baglamiyla terminal sonuca goturur.
// sistemdeki gorevi: Tenant/cancellation/UOW kabugunu tabandan alir; uzun parse ve comparison sirasinda uygulama transaction'i tutmaz.
public class ContractCheckExecutionBackgroundJob
    : ApiContractCheckerTenantBackgroundJob<ContractCheckExecutionJobArgs>, ITransientDependency
{
    private static readonly ContractCheckRunMapper Mapper = new();

    // Prepare, context, saf execution ve idempotent terminal gecislerin tek domain kapisi.
    private readonly ContractCheckRunExecutionManager _executionManager;

    public ContractCheckExecutionBackgroundJob(
        ContractCheckRunExecutionManager executionManager,
        ICurrentTenant currentTenant,
        IUnitOfWorkManager unitOfWorkManager,
        ICancellationTokenProvider cancellationTokenProvider)
        : base(currentTenant, unitOfWorkManager, cancellationTokenProvider)
    {
        _executionManager = executionManager;
    }

    // Run'i claim eder, UOW disinda hesaplar, terminal sonucu ayri kisa UOW'de saklar ve ancak sonrasinda bildirimi kuyruklar.
    protected override async Task ExecuteInTenantAsync(ContractCheckExecutionJobArgs args)
    {
        var shouldExecute = await RunInUnitOfWorkAsync(() => _executionManager.StartAsync(args.RunId));
        if (!shouldExecute)
        {
            return;
        }

        var executionTask = BuildAndExecuteAsync(args);
        await ((Task)executionTask).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
        if (!executionTask.IsCompletedSuccessfully)
        {
            await RunInUnitOfWorkAsync(
                () => _executionManager.FailAsync(args.RunId, ResolveErrorCode(executionTask.Exception)));
            await executionTask;
        }

        await RunInUnitOfWorkAsync(() => _executionManager.CompleteAsync(executionTask.Result));
    }

    // Kisa context UOW'sini Mapperly scope donusumunden sonra UOW'suz motor execution'ina baglar.
    private async Task<ContractCheckExecutionResultModel> BuildAndExecuteAsync(
        ContractCheckExecutionJobArgs args)
    {
        var scopeRules = Mapper.MapToScopeRules(args.ScopeRules);
        var context = await RunInUnitOfWorkAsync(
            () => _executionManager.BuildContextAsync(args.RunId, scopeRules, args.IgnoreInternal));
        return await _executionManager.ExecuteAsync(context);
    }

    // BusinessException kodunu korur, beklenmeyen exception ayrintisini kararli guvenli koda indirger.
    private static string ResolveErrorCode(AggregateException? exception)
    {
        return exception?.GetBaseException() is BusinessException businessException &&
               !string.IsNullOrWhiteSpace(businessException.Code)
            ? businessException.Code
            : ContractCheckRunExceptionCodes.ExecutionFailed;
    }
}
