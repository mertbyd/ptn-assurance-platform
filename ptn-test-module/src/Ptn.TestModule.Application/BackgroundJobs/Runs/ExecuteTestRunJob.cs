using System.Diagnostics;
using System.Threading.Tasks;
using Ptn.TestModule.BackgroundJobs.Runs;
using Ptn.TestModule.BackgroundJobs.Shared;
using Ptn.TestModule.Interface.Runs;
using Ptn.TestModule.Managers.Runs;
using Ptn.TestModule.Models.Runs;
using Ptn.TestModule.Services.Runs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;
using Volo.Abp.Uow;

namespace Ptn.TestModule.BackgroundJobs.Runs;

// islevi: Pending kosumu kuyruktan alir, UoW disinda icra eder, yargilar ve terminal hukmu ayri UoW'da yazar.
// sistemdeki gorevi: ADR-0015 §B akisinin tamamidir; checker cagrisi sirasinda uygulama transaction'i acik tutmaz.
public class ExecuteTestRunJob
    : TestModuleTenantBackgroundJob<ExecuteTestRunArgs>, ITransientDependency
{
    // Claim, hazirlik ve terminal yazim adimlarinin tek domain kapisidir.
    private readonly TestRunExecutionManager _executionManager;

    // Belge kabul kapisi, severity haritasi ve runner plani sahibidir.
    private readonly WorkflowRunPlanner _planner;

    // Arazzo is akisini pinli dis runner surecinde icra eden porttur.
    private readonly IWorkflowRunnerPort _runnerPort;

    // HAR artefaktini kalici BLOB deposuna yazan sinirdir.
    private readonly IHarArtifactStore _harArtifactStore;

    // HAR'i uc hakeme dagitip bulgulari toplayan yargi siniridir.
    private readonly IOracleDispatchPort _dispatchPort;

    // Adim hukumlerini ve beklenmeyen hatalari terminal hukme ceviren Manager'dir.
    private readonly RunOutcomeResolver _outcomeResolver;

    // SUT test verisini checker kimliginden ayri yazma yetkili baglantiyla sifirlayan porttur.
    private readonly ITestDataSandbox _testDataSandbox;

    public ExecuteTestRunJob(
        TestRunExecutionManager executionManager,
        WorkflowRunPlanner planner,
        IWorkflowRunnerPort runnerPort,
        IHarArtifactStore harArtifactStore,
        IOracleDispatchPort dispatchPort,
        RunOutcomeResolver outcomeResolver,
        ITestDataSandbox testDataSandbox,
        ICurrentTenant currentTenant,
        IUnitOfWorkManager unitOfWorkManager,
        ICancellationTokenProvider cancellationTokenProvider)
        : base(currentTenant, unitOfWorkManager, cancellationTokenProvider)
    {
        _executionManager = executionManager;
        _planner = planner;
        _runnerPort = runnerPort;
        _harArtifactStore = harArtifactStore;
        _dispatchPort = dispatchPort;
        _outcomeResolver = outcomeResolver;
        _testDataSandbox = testDataSandbox;
    }

    // Kosumu claim eder, UoW disinda icra ve yargi yapar, hukmu ayri yeni UoW'da kalicilastirir.
    protected override async Task ExecuteInTenantAsync(ExecuteTestRunArgs args)
    {
        var claimed = await RunInUnitOfWorkAsync(
            () => _executionManager.ClaimAsync(args.TestRunId, JobCancellationToken));
        if (!claimed)
        {
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var judgementTask = RunAndJudgeAsync(args);
        await ((Task)judgementTask).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
        stopwatch.Stop();

        var judgement = judgementTask.IsCompletedSuccessfully
            ? judgementTask.Result
            : _outcomeResolver.ResolveFailure(judgementTask.Exception);
        await RunInUnitOfWorkAsync(() => _executionManager.WriteTerminalAsync(
            args.TestRunId,
            judgement.Terminal,
            stopwatch.ElapsedMilliseconds,
            judgement.HarBlobName,
            JobCancellationToken));
    }

    // Kisa hazirlik UoW'sunu, UoW'suz runner icrasini ve UoW'suz yargiyi sirayla baglar.
    private async Task<TestRunJudgement> RunAndJudgeAsync(ExecuteTestRunArgs args)
    {
        var context = await RunInUnitOfWorkAsync(
            () => _executionManager.PrepareAsync(args.TestRunId, JobCancellationToken));
        if (context.MaterialDrift.HasDrift)
        {
            return _outcomeResolver.ResolveMaterialDrift(context.MaterialDrift);
        }

        await _testDataSandbox.ResetAsync(
            context.EnvironmentBinding.EnvironmentKey,
            JobCancellationToken);
        var outcome = await ExecuteAsync(context);
        var harBlobName = await StoreHarAsync(context, outcome);
        return await _dispatchPort.JudgeAsync(context, outcome, harBlobName, JobCancellationToken);
    }

    // Belgeyi kabul kapisindan gecirip pinli runner surecinde icra eder.
    private async Task<WorkflowRunOutcome> ExecuteAsync(TestRunExecutionContext context)
    {
        var request = await _planner.CreateRequestAsync(context, JobCancellationToken);
        return await _runnerPort.ExecuteAsync(request, JobCancellationToken);
    }

    // Artefakti Manager'in urettigi kararli blob adiyla kalici depoya yazar.
    private Task<string> StoreHarAsync(TestRunExecutionContext context, WorkflowRunOutcome outcome)
    {
        return _harArtifactStore.SaveAsync(
            WorkflowRunPlanner.CreateHarBlobName(context.TenantId, context.TestRunId, context.TraceId),
            outcome.HarContent,
            JobCancellationToken);
    }
}
