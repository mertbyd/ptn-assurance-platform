using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Ptn.TestModule.Constants.Catalog;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.Interface.Runs;
using Ptn.TestModule.Settings;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;
using Volo.Abp.Threading;
using Volo.Abp.Uow;

namespace Ptn.TestModule.BackgroundWorkers.Runs;

// islevi: Senaryo saglik materialized view'ini belirli araliklarla okuyuculari bloklamadan yeniden hesaplar.
// sistemdeki gorevi: Pass/fail/flaky ve p95 hesabinin tek sahibi veritabanidir; bu worker yalniz tazeligi yonetir.
public class ScenarioHealthRefreshWorker : AsyncPeriodicBackgroundWorkerBase, ISingletonDependency
{
    public ScenarioHealthRefreshWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory)
        : base(timer, serviceScopeFactory)
    {
        // Ilk tik sabit varsayilanla kurulur; ABP Setting scoped oldugu icin gercek periyot her tikte yenilenir.
        Timer.Period = (int)TimeSpan
            .FromSeconds(TestModuleRunSettingNames.FallbackScenarioHealthRefreshPeriodSeconds)
            .TotalMilliseconds;
    }

    // Yenilemeyi islemsiz bir UOW icinde calistirir; REFRESH ... CONCURRENTLY transaction icinde kosamaz.
    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        var settingProvider = workerContext.ServiceProvider.GetRequiredService<ISettingProvider>();
        var unitOfWorkManager = workerContext.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
        var repository = workerContext.ServiceProvider.GetRequiredService<IScenarioHealthRepository>();

        Timer.Period = await ResolvePeriodAsync(settingProvider);

        using var unitOfWork = unitOfWorkManager.Begin(requiresNew: true, isTransactional: false);
        await repository.RefreshAsync();
        await unitOfWork.CompleteAsync();
    }

    // Yenileme sikligini tenant-ustu ayardan okur ve Timer.Period'in kabul ettigi milisaniye tavanina baglar.
    private static async Task<int> ResolvePeriodAsync(ISettingProvider settingProvider)
    {
        var seconds = await settingProvider.GetAsync(
            TestModuleSettings.ScenarioHealthRefreshPeriodSeconds,
            TestModuleRunSettingNames.FallbackScenarioHealthRefreshPeriodSeconds);
        var bounded = Math.Clamp(seconds, 1, TestModuleCatalogSettingNames.MaxWorkerPeriodSeconds);
        return (int)TimeSpan.FromSeconds(bounded).TotalMilliseconds;
    }
}
