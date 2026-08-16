using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ptn.ApiContractChecker.BackgroundJobs.Sources;
using Ptn.ApiContractChecker.Configuration;
using Ptn.ApiContractChecker.Interface.Sources;
using Ptn.ApiContractChecker.Models.Sources;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace Ptn.ApiContractChecker.Application.BackgroundWorkers.Sources;

// islevi: Belirli araliklarla vadesi gelmis izlenen dokumanlari tarar ve her biri icin kontrol job'i kuyruklar.
// sistemdeki gorevi: "Hangi dokuman ne zaman" sorusunun tek sahibidir; cekim, ayristirma ve karsilastirma bu tikte calismaz.
public class DueSpecDocumentCheckWorker : AsyncPeriodicBackgroundWorkerBase, ISingletonDependency
{
    // Tarama sikligi ve tik basi dokuman tavani yapilandirmadan gelir.
    private readonly SpecMonitoringOptions _options;

    public DueSpecDocumentCheckWorker(
        AbpAsyncTimer timer,
        IServiceScopeFactory serviceScopeFactory,
        IOptions<SpecMonitoringOptions> options)
        : base(timer, serviceScopeFactory)
    {
        _options = options.Value;
        Timer.Period = (int)TimeSpan.FromSeconds(_options.WorkerPeriodSeconds).TotalMilliseconds;
    }

    // Vadesi gelmisleri tek kisa UOW ve tek sorguda okur, her birini kendi tenant baglaminda kuyruklar.
    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        var unitOfWorkManager = workerContext.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
        var sourceRepository = workerContext.ServiceProvider.GetRequiredService<ISpecSourceRepository>();
        var backgroundJobManager = workerContext.ServiceProvider.GetRequiredService<IBackgroundJobManager>();
        var currentTenant = workerContext.ServiceProvider.GetRequiredService<ICurrentTenant>();
        var clock = workerContext.ServiceProvider.GetRequiredService<IClock>();

        using var unitOfWork = unitOfWorkManager.Begin(requiresNew: true);
        var dueDocuments = await sourceRepository.GetDueMonitoredDocumentsAsync(
            clock.Now,
            _options.MaxDocumentsPerTick);

        await EnqueueAllAsync(backgroundJobManager, currentTenant, dueDocuments);
        await unitOfWork.CompleteAsync();
    }

    // Tarama sonucunu kuyruk kayitlarina cevirir; tenant her kayitta ayri ayri geri acilir.
    // Dongude OKUMA yoktur: dokumanlar tek sorguyla zaten bellekte (regresyonu SQL yakalama testi korur).
    // Kuyruga her dokuman icin bir kayit dusmesi isin tanimidir; ABP toplu enqueue sunmaz ve yazimlar
    // autoSave yerine cagiran UOW'un tek flush'ina birakilir. Bu dongu bilincli bir N+1 istisnasidir.
    private static async Task EnqueueAllAsync(
        IBackgroundJobManager backgroundJobManager,
        ICurrentTenant currentTenant,
        List<DueSpecDocumentModel> dueDocuments)
    {
        foreach (var document in dueDocuments)
        {
            using (currentTenant.Change(document.TenantId))
            {
                await backgroundJobManager.EnqueueAsync(new ScheduledDocumentCheckJobArgs
                {
                    SpecSourceId = document.SpecSourceId,
                    SpecDocumentId = document.SpecDocumentId,
                    TenantId = document.TenantId
                });
            }
        }
    }
}
