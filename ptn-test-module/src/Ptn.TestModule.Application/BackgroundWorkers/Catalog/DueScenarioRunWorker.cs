using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Ptn.TestModule.BackgroundJobs.Runs;
using Ptn.TestModule.Constants.Catalog;
using Ptn.TestModule.Constants.Runs.Lookups;
using Ptn.TestModule.Interface.Catalog;
using Ptn.TestModule.Interface.Runs;
using Ptn.TestModule.Managers.Catalog;
using Ptn.TestModule.Managers.Runs;
using Ptn.TestModule.Models.Runs;
using Ptn.TestModule.Settings;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Settings;
using Volo.Abp.Threading;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace Ptn.TestModule.BackgroundWorkers.Catalog;

// islevi: Belirli araliklarla vadesi gelmis zamanlanmis senaryolari tarar ve her biri icin kosum kuyruklar.
// sistemdeki gorevi: "Hangi senaryo ne zaman" sorusunun tek sahibidir; kosumun kendisi bu tikte calismaz (PLAN-0003 TM-29).
public class DueScenarioRunWorker : AsyncPeriodicBackgroundWorkerBase, ISingletonDependency
{
    public DueScenarioRunWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory)
        : base(timer, serviceScopeFactory)
    {
        // Ilk tik sabit varsayilanla kurulur; ABP Setting scoped oldugu icin gercek periyot her tikte yenilenir.
        Timer.Period = (int)TimeSpan
            .FromSeconds(TestModuleCatalogSettingNames.FallbackScheduleSweepPeriodSeconds)
            .TotalMilliseconds;
    }

    // Vadesi gelmisleri tek kisa UOW ve tek sorguda okur, her senaryoyu kendi tenant baglaminda kuyruklar.
    // Dongude TARAMA sorgusu yoktur: senaryolar tek sorguyla zaten bellektedir. Kuyruga her senaryo icin
    // bir kayit dusmesi isin tanimidir; ABP toplu enqueue sunmaz. Bu dongu bilincli bir N+1 istisnasidir.
    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        var settingProvider = workerContext.ServiceProvider.GetRequiredService<ISettingProvider>();
        var unitOfWorkManager = workerContext.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
        var scenarioRepository = workerContext.ServiceProvider.GetRequiredService<ITestScenarioRepository>();
        var runRepository = workerContext.ServiceProvider.GetRequiredService<ITestRunRepository>();
        var triggerManager = workerContext.ServiceProvider.GetRequiredService<AutomatedRunTriggerManager>();
        var scheduleManager = workerContext.ServiceProvider.GetRequiredService<ScenarioScheduleManager>();
        var backgroundJobManager = workerContext.ServiceProvider.GetRequiredService<IBackgroundJobManager>();
        var currentTenant = workerContext.ServiceProvider.GetRequiredService<ICurrentTenant>();
        var clock = workerContext.ServiceProvider.GetRequiredService<IClock>();

        Timer.Period = await ResolvePeriodAsync(settingProvider);
        var maxScenariosPerTick = await settingProvider.GetAsync<int>(TestModuleSettings.MaxScenariosPerTick);

        using var unitOfWork = unitOfWorkManager.Begin(requiresNew: true);
        var dueScenarios = await scenarioRepository.GetDueScheduledAsync(clock.Now, maxScenariosPerTick);
        foreach (var due in dueScenarios)
        {
            using (currentTenant.Change(due.TenantId))
            {
                var outcome = await triggerManager.ResolveAsync(new AutomatedRunRequest
                {
                    ScenarioId = due.ScenarioId,
                    ScenarioKey = due.ScenarioKey,
                    TriggerKindCode = TestTriggerKindCodes.Scheduled,
                    TriggerRef = ScenarioScheduleManager.CreateTriggerRef(due.ScenarioId, due.NextRunAt),
                    EnvironmentKey = await triggerManager.ResolveAutomationEnvironmentKeyAsync(),
                    CanonicalInputs = due.CompiledHash
                });

                await EnqueueAsync(runRepository, backgroundJobManager, outcome);
            }
        }

        // Vade ilerletme donguden sonra tek okuma ve tek yazma ile yapilir; senaryo basina sorgu acilmaz.
        await scheduleManager.AdvanceManyAsync(
            [.. dueScenarios.Select(due => due.ScenarioId)],
            clock.Now);
        await unitOfWork.CompleteAsync();
    }

    // Yeni uretilen Pending kosumu kalicilastirip dayanikli icra job'ini kuyruga verir.
    // Ayni vade daha once kosum urettiyse outcome mevcut kaydi tasir ve ikinci kuyruk kaydi acilmaz.
    private static async Task EnqueueAsync(
        ITestRunRepository runRepository,
        IBackgroundJobManager backgroundJobManager,
        AutomatedRunOutcome outcome)
    {
        if (!outcome.IsNew)
        {
            return;
        }

        var saved = await runRepository.InsertAsync(outcome.Run, autoSave: true);
        await backgroundJobManager.EnqueueAsync(new ExecuteTestRunArgs
        {
            TestRunId = saved.Id,
            TenantId = saved.TenantId,
            TraceId = saved.TraceId ?? string.Empty
        });
    }

    // Tarama sikligini tenant-ustu ayardan okur ve Timer.Period'in kabul ettigi milisaniye tavanina baglar.
    private static async Task<int> ResolvePeriodAsync(ISettingProvider settingProvider)
    {
        var seconds = await settingProvider.GetAsync(
            TestModuleSettings.ScheduleSweepPeriodSeconds,
            TestModuleCatalogSettingNames.FallbackScheduleSweepPeriodSeconds);
        var bounded = Math.Clamp(seconds, 1, TestModuleCatalogSettingNames.MaxWorkerPeriodSeconds);
        return (int)TimeSpan.FromSeconds(bounded).TotalMilliseconds;
    }
}
