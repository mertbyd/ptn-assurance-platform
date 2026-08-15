using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Ptn.TestModule.Constants.Catalog;
using Ptn.TestModule.Entities.Catalog;
using Ptn.TestModule.Interface.Catalog;
using Ptn.TestModule.Managers.Runs;
using Ptn.TestModule.Settings;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;
using Volo.Abp.Threading;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace Ptn.TestModule.BackgroundWorkers.Catalog;

// islevi: Belirli araliklarla suresi dolmus senaryo karantinalarini tarar ve mevcut Manager yoluyla temizler.
// sistemdeki gorevi: "Karantina ne zaman biter" sorusunu elle mudahaleden ayirir; yeni karar mantigi tasimaz (PLAN-0003 TM-28).
public class ExpiredQuarantineSweepWorker : AsyncPeriodicBackgroundWorkerBase, ISingletonDependency
{
    public ExpiredQuarantineSweepWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory)
        : base(timer, serviceScopeFactory)
    {
        // Ilk tik sabit varsayilanla kurulur; ABP Setting scoped oldugu icin gercek periyot her tikte yenilenir.
        Timer.Period = (int)TimeSpan
            .FromSeconds(TestModuleCatalogSettingNames.FallbackQuarantineSweepPeriodSeconds)
            .TotalMilliseconds;
    }

    // Suresi dolmuslari tek kisa UOW ve tek sorguda okur, karar vermeden mevcut Manager'a devreder.
    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        var settingProvider = workerContext.ServiceProvider.GetRequiredService<ISettingProvider>();
        var unitOfWorkManager = workerContext.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
        var repository = workerContext.ServiceProvider.GetRequiredService<ITestScenarioRepository>();
        var quarantineManager = workerContext.ServiceProvider.GetRequiredService<ScenarioQuarantineManager>();
        var clock = workerContext.ServiceProvider.GetRequiredService<IClock>();

        Timer.Period = await ResolvePeriodAsync(settingProvider);
        var batchSize = await settingProvider.GetAsync<int>(TestModuleSettings.QuarantineSweepBatchSize);

        using var unitOfWork = unitOfWorkManager.Begin(requiresNew: true);
        var expired = await repository.GetExpiredQuarantinesAsync(clock.Now, batchSize);
        ReleaseAll(quarantineManager, expired, clock.Now);
        await repository.UpdateManyAsync(expired, autoSave: true);
        await unitOfWork.CompleteAsync();
    }

    // Tarama sikligini tenant-ustu ayardan okur ve Timer.Period'in kabul ettigi milisaniye tavanina baglar.
    private static async Task<int> ResolvePeriodAsync(ISettingProvider settingProvider)
    {
        var seconds = await settingProvider.GetAsync(
            TestModuleSettings.QuarantineSweepPeriodSeconds,
            TestModuleCatalogSettingNames.FallbackQuarantineSweepPeriodSeconds);
        var bounded = Math.Clamp(seconds, 1, TestModuleCatalogSettingNames.MaxWorkerPeriodSeconds);
        return (int)TimeSpan.FromSeconds(bounded).TotalMilliseconds;
    }

    // Sure kararini vermeden her satiri mevcut karantina kuralina sunar; kural degismisse satir el degmeden kalir.
    private static void ReleaseAll(
        ScenarioQuarantineManager quarantineManager,
        IReadOnlyList<TestScenario> scenarios,
        DateTime now)
    {
        foreach (var scenario in scenarios)
        {
            quarantineManager.ReleaseExpired(scenario, now);
        }
    }
}
