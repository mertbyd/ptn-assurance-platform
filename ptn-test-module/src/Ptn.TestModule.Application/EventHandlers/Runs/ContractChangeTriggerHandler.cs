using System.Collections.Generic;
using System.Threading.Tasks;
using Ptn.ApiContractChecker.Constants.Differences.Lookups;
using Ptn.ApiContractChecker.Constants.Runs.Lookups;
using Ptn.ApiContractChecker.Dtos.Runs;
using Ptn.ApiContractChecker.Events.Runs;
using Ptn.ApiContractChecker.Services.Runs;
using Ptn.TestModule.BackgroundJobs.Runs;
using Ptn.TestModule.Constants.Runs.Lookups;
using Ptn.TestModule.Interface.Runs;
using Ptn.TestModule.Managers.Runs;
using Ptn.TestModule.Models.Catalog;
using Ptn.TestModule.Models.Runs;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace Ptn.TestModule.EventHandlers.Runs;

// islevi: API Checker kosum durum olayini dinler ve eski sozlesmeye muhurlu senaryolar icin kosum kuyruklar.
// sistemdeki gorevi: Olay ile olgu dinlenir, detay checker'in kendi AppService'inden cekilir; checker tablosu
// okunmaz ve FK verilmez (ADR-0015 §F, ADR-0007).
public class ContractChangeTriggerHandler
    : ILocalEventHandler<ContractCheckRunStatusChangedEto>, ITransientDependency
{
    private readonly IContractCheckRunAppService _contractCheckRunAppService;
    private readonly ContractChangeImpactManager _impactManager;
    private readonly AutomatedRunTriggerManager _triggerManager;
    private readonly ITestRunRepository _testRunRepository;
    private readonly IBackgroundJobManager _backgroundJobManager;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly ICurrentTenant _currentTenant;
    private readonly IClock _clock;

    public ContractChangeTriggerHandler(
        IContractCheckRunAppService contractCheckRunAppService,
        ContractChangeImpactManager impactManager,
        AutomatedRunTriggerManager triggerManager,
        ITestRunRepository testRunRepository,
        IBackgroundJobManager backgroundJobManager,
        IUnitOfWorkManager unitOfWorkManager,
        ICurrentTenant currentTenant,
        IClock clock)
    {
        _contractCheckRunAppService = contractCheckRunAppService;
        _impactManager = impactManager;
        _triggerManager = triggerManager;
        _testRunRepository = testRunRepository;
        _backgroundJobManager = backgroundJobManager;
        _unitOfWorkManager = unitOfWorkManager;
        _currentTenant = currentTenant;
        _clock = clock;
    }

    // Olayin agirlik ozetini once okur; breaking + New olmayan gecislerde checker'a hic gidilmez.
    public async Task HandleEventAsync(ContractCheckRunStatusChangedEto eventData)
    {
        var signal = new ContractChangeSignal
        {
            CheckRunId = eventData.RunId,
            TenantId = eventData.TenantId,
            StatusCode = eventData.StatusCode,
            NewFindingCount = eventData.NewFindingCount,
            MaxSeverityCode = eventData.MaxSeverityCode
        };
        if (!ContractChangeImpactManager.IsActionable(signal))
        {
            return;
        }

        using (_currentTenant.Change(signal.TenantId))
        {
            await TriggerAffectedScenariosAsync(signal);
        }
    }

    // Detayi checker'in kendi okuma yuzeyinden dogrular, sonra etkilenen senaryolari kendi UOW'unda kuyruklar.
    private async Task TriggerAffectedScenariosAsync(ContractChangeSignal signal)
    {
        var breakingFindings = await _contractCheckRunAppService.GetFindingsAsync(
            signal.CheckRunId,
            new GetContractCheckFindingsInput
            {
                ChangeStateCode = FindingChangeStateCodes.New,
                SeverityCode = DifferenceSeverityCodes.Breaking,
                MaxResultCount = 1
            });
        if (breakingFindings.TotalCount == 0)
        {
            return;
        }

        var checkRun = await _contractCheckRunAppService.GetAsync(signal.CheckRunId);
        using var unitOfWork = _unitOfWorkManager.Begin(requiresNew: true);
        var affected = await _impactManager.GetAffectedScenariosAsync(checkRun.BaseSnapshotId, _clock.Now);
        await EnqueueAllAsync(affected, ContractChangeImpactManager.CreateTriggerRef(signal.CheckRunId));
        await unitOfWork.CompleteAsync();
    }

    // Etkilenen her senaryo icin bir kosum kuyruklar; ayni (checkRunId, scenarioId) cifti ikinci kez uretmez.
    // Dongude TARAMA sorgusu yoktur: senaryolar tek sorguyla zaten bellektedir. Kuyruga senaryo basina bir
    // kayit dusmesi isin tanimidir; ABP toplu enqueue sunmaz. Bu dongu bilincli bir N+1 istisnasidir.
    private async Task EnqueueAllAsync(IReadOnlyList<DueScenarioModel> affected, string triggerRef)
    {
        var environmentKey = await _triggerManager.ResolveAutomationEnvironmentKeyAsync();
        foreach (var scenario in affected)
        {
            var outcome = await _triggerManager.ResolveAsync(new AutomatedRunRequest
            {
                ScenarioId = scenario.ScenarioId,
                ScenarioKey = scenario.ScenarioKey,
                TriggerKindCode = TestTriggerKindCodes.ContractChange,
                TriggerRef = triggerRef,
                EnvironmentKey = environmentKey,
                CanonicalInputs = scenario.CompiledHash
            });
            await EnqueueAsync(outcome);
        }
    }

    // Yalniz yeni uretilen kosum kalicilastirilir ve kuyruga verilir.
    private async Task EnqueueAsync(AutomatedRunOutcome outcome)
    {
        if (!outcome.IsNew)
        {
            return;
        }

        var saved = await _testRunRepository.InsertAsync(outcome.Run, autoSave: true);
        await _backgroundJobManager.EnqueueAsync(new ExecuteTestRunArgs
        {
            TestRunId = saved.Id,
            TenantId = saved.TenantId,
            TraceId = saved.TraceId ?? string.Empty
        });
    }
}
