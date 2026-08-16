using System.Net;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ptn.ApiContractChecker.Application.BackgroundJobs.Sources;
using Ptn.ApiContractChecker.Application.BackgroundWorkers.Sources;
using Ptn.ApiContractChecker.BackgroundJobs.Sources;
using Ptn.ApiContractChecker.Configuration;
using Ptn.ApiContractChecker.Constants.Sources;
using Ptn.ApiContractChecker.Dtos.Sources;
using Ptn.ApiContractChecker.Entities.Sources;
using Ptn.ApiContractChecker.Interface.Runs;
using Ptn.ApiContractChecker.Interface.Snapshots;
using Ptn.ApiContractChecker.Interface.Sources;
using Ptn.ApiContractChecker.Managers.Sources;
using Ptn.ApiContractChecker.Services.Sources;
using Shouldly;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Data;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;
using Volo.Abp.Timing;
using Xunit;

namespace Ptn.ApiContractChecker.EntityFrameworkCore.Sources;

// islevi: Zamanlanmis izlemenin vade secimi, capraz tenant taramasi, degisim sinyali ve idempotansini gercek EF altyapisiyla dogrular.
// sistemdeki gorevi: KBP-620 sozlesmesini sabitler; worker'in yalniz kuyrukladigini, calistirmanin ancak icerik degistiginde acildigini ve erisilemeyen servisin dokumani susturmadigini kanitlar.
[Collection(EfCoreIntegrationCollection.Name)]
public class ScheduledDocumentMonitoring_Tests : ApiContractCheckerEntityFrameworkCoreTestBase
{
    // Test modulunun 64 baytlik cekim sinirinin altinda kalan gecerli OAS 3.1 dokumani.
    private const string CompactSpec = "{\"openapi\":\"3.1.0\",\"info\":{\"title\":\"o\",\"version\":\"1\"}}";

    // Ayni dokumanin yalniz surumu degismis ikinci hali; yine sinir altinda kalir.
    private const string ChangedSpec = "{\"openapi\":\"3.1.0\",\"info\":{\"title\":\"o\",\"version\":\"2\"}}";

    private const int IntervalMinutes = 15;

    private readonly ISpecSourceAppService _appService;
    private readonly ISpecSourceRepository _sourceRepository;
    private readonly SpecSourceManager _sourceManager;
    private readonly ISpecSnapshotRepository _snapshotRepository;
    private readonly IContractCheckRunRepository _runRepository;
    private readonly ScheduledDocumentCheckJob _job;
    private readonly SpecSourceHttpClientFactory _httpClientFactory;
    private readonly IBackgroundJobStore _jobStore;
    private readonly IDataFilter<IMultiTenant> _multiTenantFilter;
    private readonly ICurrentTenant _currentTenant;
    private readonly SqlCommandCaptureInterceptor _commandCapture;

    public ScheduledDocumentMonitoring_Tests()
    {
        _appService = GetRequiredService<ISpecSourceAppService>();
        _sourceRepository = GetRequiredService<ISpecSourceRepository>();
        _sourceManager = GetRequiredService<SpecSourceManager>();
        _snapshotRepository = GetRequiredService<ISpecSnapshotRepository>();
        _runRepository = GetRequiredService<IContractCheckRunRepository>();
        _job = GetRequiredService<ScheduledDocumentCheckJob>();
        _httpClientFactory = (SpecSourceHttpClientFactory)GetRequiredService<IHttpClientFactory>();
        _jobStore = GetRequiredService<IBackgroundJobStore>();
        _multiTenantFilter = GetRequiredService<IDataFilter<IMultiTenant>>();
        _currentTenant = GetRequiredService<ICurrentTenant>();
        _commandCapture = GetRequiredService<SqlCommandCaptureInterceptor>();
    }

    // Worker'in yalniz izlenen ve vadesi gelmis dokumani kuyrukladigini kanitlar.
    [Fact]
    public async Task Worker_Should_Queue_Only_Documents_That_Are_Monitored_And_Due()
    {
        var tenantId = Guid.NewGuid();
        var due = await CreateSourceAsync(tenantId, "due");
        var notYetDue = await CreateSourceAsync(tenantId, "not-yet-due");
        var unmonitored = await CreateSourceAsync(tenantId, "unmonitored");

        await EnableMonitoringAsync(tenantId, due);
        await EnableMonitoringAsync(tenantId, notYetDue);
        await PushDueDateForwardAsync(tenantId, notYetDue);

        await RunWorkerTickAsync();

        var queued = await GetQueuedCheckArgsAsync();
        queued.Select(args => args.SpecDocumentId).ShouldBe([due.DocumentId]);
        queued.ShouldNotContain(args => args.SpecDocumentId == unmonitored.DocumentId);
    }

    // Iki tenant'in dokumanlarinin her birinin kendi tenant baglamiyla kuyruga girdigini kanitlar.
    [Fact]
    public async Task Worker_Should_Queue_Every_Tenant_Document_Under_Its_Own_Tenant()
    {
        var firstTenantId = Guid.NewGuid();
        var secondTenantId = Guid.NewGuid();
        var first = await CreateSourceAsync(firstTenantId, "first");
        var second = await CreateSourceAsync(secondTenantId, "second");
        await EnableMonitoringAsync(firstTenantId, first);
        await EnableMonitoringAsync(secondTenantId, second);

        _commandCapture.Clear();
        await RunWorkerTickAsync();

        var queued = await GetQueuedCheckArgsAsync();
        queued.Count.ShouldBe(2);
        queued.Single(args => args.SpecDocumentId == first.DocumentId).TenantId.ShouldBe(firstTenantId);
        queued.Single(args => args.SpecDocumentId == second.DocumentId).TenantId.ShouldBe(secondTenantId);

        // Tarama tek sorgudur: dokuman sayisi artsa da tikte dokuman basina okuma yapilmaz.
        _commandCapture.Commands
            .Count(command => command.Contains("spec_documents", StringComparison.OrdinalIgnoreCase))
            .ShouldBe(1);
    }

    // Icerik degismediginde calistirma acilmadigini ama vadenin yine de ilerledigini kanitlar.
    [Fact]
    public async Task Unchanged_Content_Should_Advance_The_Schedule_Without_Opening_A_Run()
    {
        var tenantId = Guid.NewGuid();
        var fixture = await CreateSourceAsync(tenantId, "orders");
        await EnableMonitoringAsync(tenantId, fixture);

        ConfigureSpecResponse(CompactSpec);
        await _job.ExecuteAsync(BuildArgs(tenantId, fixture));
        ConfigureSpecResponse(CompactSpec);
        await _job.ExecuteAsync(BuildArgs(tenantId, fixture));

        using (_currentTenant.Change(tenantId))
        {
            (await _snapshotRepository.GetCountAsync()).ShouldBe(1);
            (await _runRepository.GetCountAsync()).ShouldBe(0);
        }

        var document = await GetDocumentAsync(tenantId, fixture);
        document.LastCheckedAt.ShouldNotBeNull();
        document.NextCheckAt.ShouldBe(document.LastCheckedAt!.Value.AddMinutes(IntervalMinutes));
        document.LastFetchOutcome.ShouldBe(SpecDocumentFetchOutcomeCodes.Unchanged);
    }

    // Icerik degistiginde yeni snapshot ile bir onceki snapshot'in karsilastirmaya girdigini kanitlar.
    [Fact]
    public async Task Changed_Content_Should_Queue_A_Comparison_Against_The_Previous_Snapshot()
    {
        var tenantId = Guid.NewGuid();
        var fixture = await CreateSourceAsync(tenantId, "orders");
        await EnableMonitoringAsync(tenantId, fixture);

        ConfigureSpecResponse(CompactSpec);
        await _job.ExecuteAsync(BuildArgs(tenantId, fixture));
        var firstSnapshotId = await GetLatestSnapshotIdAsync(tenantId, fixture);

        ConfigureSpecResponse(ChangedSpec);
        await _job.ExecuteAsync(BuildArgs(tenantId, fixture));
        var secondSnapshotId = await GetLatestSnapshotIdAsync(tenantId, fixture);

        secondSnapshotId.ShouldNotBe(firstSnapshotId);
        (await GetDocumentAsync(tenantId, fixture)).LastFetchOutcome
            .ShouldBe(SpecDocumentFetchOutcomeCodes.Changed);
        Guid runId;
        using (_currentTenant.Change(tenantId))
        {
            var run = (await _runRepository.GetListAsync()).ShouldHaveSingleItem();
            run.BaseSnapshotId.ShouldBe(firstSnapshotId);
            run.TargetSnapshotId.ShouldBe(secondSnapshotId);
            runId = run.Id;
        }

        // Karsilastirmayi bu job calistirmaz; mevcut execution job'ina devreder.
        (await GetQueuedExecutionPayloadsAsync())
            .ShouldContain(payload => payload.Contains(runId.ToString(), StringComparison.OrdinalIgnoreCase));
    }

    // Ayni job payload'inin iki kez teslim edilmesinin ikinci bir calistirma acmadigini kanitlar.
    [Fact]
    public async Task Duplicate_Job_Delivery_Should_Not_Open_A_Second_Run()
    {
        var tenantId = Guid.NewGuid();
        var fixture = await CreateSourceAsync(tenantId, "orders");
        await EnableMonitoringAsync(tenantId, fixture);
        var args = BuildArgs(tenantId, fixture);

        ConfigureSpecResponse(CompactSpec);
        await _job.ExecuteAsync(args);
        ConfigureSpecResponse(ChangedSpec);
        await _job.ExecuteAsync(args);
        ConfigureSpecResponse(ChangedSpec);
        await _job.ExecuteAsync(args);

        using (_currentTenant.Change(tenantId))
        {
            (await _runRepository.GetCountAsync()).ShouldBe(1);
            (await _snapshotRepository.GetCountAsync()).ShouldBe(2);
        }
    }

    // Erisilemeyen servisin dokumani susturmadigini: vade ilerler, calistirma acilmaz.
    [Fact]
    public async Task Failed_Fetch_Should_Advance_The_Schedule_And_Open_No_Run()
    {
        var tenantId = Guid.NewGuid();
        var fixture = await CreateSourceAsync(tenantId, "orders");
        await EnableMonitoringAsync(tenantId, fixture);
        _httpClientFactory.ConfigureException(new HttpRequestException("No such host is known (orders.test:443)"));

        // Cekim hattinda tasima hatasi bilerek yutulmaz; yeniden deneme sinirinin sahibi ABP job altyapisidir.
        await Should.ThrowAsync<HttpRequestException>(() => _job.ExecuteAsync(BuildArgs(tenantId, fixture)));

        var document = await GetDocumentAsync(tenantId, fixture);
        document.IsMonitored.ShouldBeTrue();
        document.LastCheckedAt.ShouldNotBeNull();
        document.NextCheckAt.ShouldBe(document.LastCheckedAt!.Value.AddMinutes(IntervalMinutes));
        document.LastFetchOutcome.ShouldBe(SpecDocumentFetchOutcomeCodes.Unreachable);
        using (_currentTenant.Change(tenantId))
        {
            (await _runRepository.GetCountAsync()).ShouldBe(0);
        }
    }

    // Periyodik tarayicinin tek tikini timer'i baslatmadan deterministik olarak calistirir.
    private async Task RunWorkerTickAsync()
    {
        var worker = new TestableDueSpecDocumentCheckWorker(
            GetRequiredService<AbpAsyncTimer>(),
            GetRequiredService<IServiceScopeFactory>(),
            GetRequiredService<IOptions<SpecMonitoringOptions>>());

        await worker.RunTickAsync(ServiceProvider);
    }

    // Tenant sahipligi altinda tek dokumanli bir kaynak kurar.
    private async Task<MonitoredDocumentFixture> CreateSourceAsync(Guid tenantId, string name)
    {
        var sourceId = Guid.NewGuid();
        var documentId = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () =>
        {
            using (_currentTenant.Change(tenantId))
            {
                var source = new SpecSource(sourceId, name, "https://orders.test", null, tenantId);
                _sourceManager.AddDocument(source, documentId, "v1", "/openapi/v1.json");
                await _sourceRepository.InsertAsync(source, autoSave: true);
            }
        });

        return new MonitoredDocumentFixture(sourceId, documentId);
    }

    // Izlemeyi gercek endpoint akisiyla acar; vade aninda gelmis olur.
    private async Task EnableMonitoringAsync(Guid tenantId, MonitoredDocumentFixture fixture)
    {
        using (_currentTenant.Change(tenantId))
        {
            await _appService.ConfigureDocumentMonitoringAsync(
                fixture.SourceId,
                fixture.DocumentId,
                new ConfigureSpecDocumentMonitoringDto
                {
                    IsMonitored = true,
                    CheckIntervalMinutes = IntervalMinutes
                });
        }
    }

    // Dokumani bir kontrol denemesi islemis gibi ileri tarihe atar; boylece bu tikte vadesi gelmemis olur.
    private async Task PushDueDateForwardAsync(Guid tenantId, MonitoredDocumentFixture fixture)
    {
        await WithUnitOfWorkAsync(async () =>
        {
            using (_currentTenant.Change(tenantId))
            {
                var source = (await _sourceRepository.FindWithDetailsAsync(fixture.SourceId)).ShouldNotBeNull();
                _sourceManager.MarkDocumentChecked(source, fixture.DocumentId, GetRequiredService<IClock>().Now);
                await _sourceRepository.UpdateAsync(source, autoSave: true);
            }
        });
    }

    // Zamanlama alanlarini dogrulamak icin dokumani aggregate uzerinden okur.
    private async Task<SpecDocument> GetDocumentAsync(Guid tenantId, MonitoredDocumentFixture fixture)
    {
        return await WithUnitOfWorkAsync(async () =>
        {
            using (_currentTenant.Change(tenantId))
            {
                var source = await _sourceRepository.FindWithDetailsAsync(fixture.SourceId);
                return source!.Documents.Single(document => document.Id == fixture.DocumentId);
            }
        });
    }

    // Dokumanin son acilan snapshot kimligini tenant baglaminda okur.
    private async Task<Guid> GetLatestSnapshotIdAsync(Guid tenantId, MonitoredDocumentFixture fixture)
    {
        return await WithUnitOfWorkAsync(async () =>
        {
            using (_currentTenant.Change(tenantId))
            {
                var snapshot = await _snapshotRepository.FindLatestForDocumentAsync(fixture.DocumentId);
                return snapshot!.Id;
            }
        });
    }

    // Kuyrukta bekleyen zamanlanmis kontrol job'larini butun tenant'lar boyunca payload'a cevirir.
    private async Task<List<ScheduledDocumentCheckJobArgs>> GetQueuedCheckArgsAsync()
    {
        var serializer = GetRequiredService<IBackgroundJobSerializer>();
        var payloads = await GetQueuedPayloadsAsync<ScheduledDocumentCheckJobArgs>();
        return payloads
            .Select(payload => (ScheduledDocumentCheckJobArgs)serializer.Deserialize(
                payload,
                typeof(ScheduledDocumentCheckJobArgs)))
            .ToList();
    }

    // Kuyrukta bekleyen karsilastirma job'larinin serialize edilmis payload'larini okur.
    private Task<List<string>> GetQueuedExecutionPayloadsAsync()
    {
        return GetQueuedPayloadsAsync<Ptn.ApiContractChecker.BackgroundJobs.Runs.ContractCheckExecutionJobArgs>();
    }

    // Verilen job tipinin bekleyen kayitlarini tenant filtresi olmadan okur; worker da tenant'siz calisir.
    private async Task<List<string>> GetQueuedPayloadsAsync<TArgs>()
    {
        var jobName = BackgroundJobNameAttribute.GetName<TArgs>();
        using (_multiTenantFilter.Disable())
        {
            // Store'un ilk parametresi uygulama adidir, job adi degil; ad filtresi bellekte uygulanir.
            var waiting = await _jobStore.GetWaitingJobsAsync(applicationName: null, int.MaxValue);
            return waiting
                .Where(job => job.JobName == jobName)
                .Select(job => job.JobArgs)
                .ToList();
        }
    }

    // Cekim adapterine verilen govdeyi basarili JSON yaniti olarak yapilandirir.
    private void ConfigureSpecResponse(string body)
    {
        _httpClientFactory.ConfigureResponse(
            HttpStatusCode.OK,
            Encoding.UTF8.GetBytes(body),
            "application/json");
    }

    // Worker'in kuyrukladigi payload'in aynisini test tarafindan kurar.
    private static ScheduledDocumentCheckJobArgs BuildArgs(Guid tenantId, MonitoredDocumentFixture fixture)
    {
        return new ScheduledDocumentCheckJobArgs
        {
            SpecSourceId = fixture.SourceId,
            SpecDocumentId = fixture.DocumentId,
            TenantId = tenantId
        };
    }

    // islevi: Bir senaryodaki kaynak ve dokuman kimliklerini birlikte tasir.
    // sistemdeki gorevi: Her testin iki Guid'i ayri ayri gezdirmesini engeller.
    private sealed record MonitoredDocumentFixture(Guid SourceId, Guid DocumentId);

    // islevi: Periyodik worker'in korumali tik metodunu teste acar.
    // sistemdeki gorevi: Timer'i baslatmadan tek tiki deterministik calistirir; uretim worker'i test icin degistirilmez.
    private sealed class TestableDueSpecDocumentCheckWorker : DueSpecDocumentCheckWorker
    {
        public TestableDueSpecDocumentCheckWorker(
            AbpAsyncTimer timer,
            IServiceScopeFactory serviceScopeFactory,
            IOptions<SpecMonitoringOptions> options)
            : base(timer, serviceScopeFactory, options)
        {
        }

        // Tek tarama tikini verilen servis saglayicisiyla calistirir.
        public Task RunTickAsync(IServiceProvider serviceProvider)
        {
            return DoWorkAsync(new PeriodicBackgroundWorkerContext(serviceProvider));
        }
    }
}
