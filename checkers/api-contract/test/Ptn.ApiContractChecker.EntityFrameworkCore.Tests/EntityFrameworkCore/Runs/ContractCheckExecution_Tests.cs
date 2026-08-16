using Microsoft.EntityFrameworkCore;
using Ptn.ApiContractChecker.Application.BackgroundJobs.Runs;
using Ptn.ApiContractChecker.BackgroundJobs.Runs;
using Ptn.ApiContractChecker.Constants.Differences.Lookups;
using Ptn.ApiContractChecker.Constants.Runs.Lookups;
using Ptn.ApiContractChecker.Dtos.Runs;
using Ptn.ApiContractChecker.Entities.Lookups;
using Ptn.ApiContractChecker.Entities.Runs;
using Ptn.ApiContractChecker.ExceptionCodes;
using Ptn.ApiContractChecker.ExceptionCodes.Runs;
using Ptn.ApiContractChecker.ExceptionCodes.Snapshots;
using Ptn.ApiContractChecker.Events.Runs;
using Ptn.ApiContractChecker.Interface.Runs;
using Ptn.ApiContractChecker.Managers.Runs;
using Ptn.ApiContractChecker.Managers.Snapshots;
using Ptn.ApiContractChecker.Managers.Sources;
using Ptn.ApiContractChecker.Models.Runs;
using Ptn.ApiContractChecker.Models.Sources;
using Ptn.ApiContractChecker.Services.Runs;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EventBus.Local;
using Xunit;

namespace Ptn.ApiContractChecker.EntityFrameworkCore.Runs;

// islevi: Asenkron contract-check tetikleme, job, failure, idempotans, stale recovery ve tenant sinirini gercek EF altyapisiyla dogrular.
// sistemdeki gorevi: KBP-615 yasam dongusunun kisa UOW ve kalici olmayan scope sozlesmesini uc katman birlikte calisirken sabitler.
[Collection(EfCoreIntegrationCollection.Name)]
public class ContractCheckExecution_Tests : ContractCheckRunTestBase
{
    private const string InvalidSpec = "{";

    private readonly ContractCheckRunExecutionManager _executionManager;
    private readonly ContractCheckRunManager _runManager;
    private readonly ContractCheckExecutionBackgroundJob _job;
    private readonly IContractCheckRunAppService _appService;
    private readonly IContractCheckRunRepository _runRepository;
    private readonly IRepository<CheckRunStatus, Guid> _statusRepository;
    private readonly SqlCommandCaptureInterceptor _commandCapture;

    public ContractCheckExecution_Tests()
    {
        _executionManager = GetRequiredService<ContractCheckRunExecutionManager>();
        _runManager = GetRequiredService<ContractCheckRunManager>();
        _job = GetRequiredService<ContractCheckExecutionBackgroundJob>();
        _appService = GetRequiredService<IContractCheckRunAppService>();
        _runRepository = GetRequiredService<IContractCheckRunRepository>();
        _statusRepository = GetRequiredService<IRepository<CheckRunStatus, Guid>>();
        _commandCapture = GetRequiredService<SqlCommandCaptureInterceptor>();
    }

    // Ayni ABP job payload'inin iki tesliminde tek terminal sonuc kaldigini ve ikinci teslimin run'i yazmadigini kanitlar.
    [Fact]
    public async Task Duplicate_Job_Delivery_Should_Keep_One_Result_And_Not_Rewrite_Terminal_Run()
    {
        var tenantId = Guid.NewGuid();
        var fixture = await CreateSnapshotGraphAsync(tenantId, BaseSpec, TargetSpec);
        var runId = await PrepareAsync(tenantId, fixture.BaseSnapshotId, fixture.TargetSnapshotId);
        var args = new ContractCheckExecutionJobArgs { RunId = runId, TenantId = tenantId };

        await _job.ExecuteAsync(args);
        _commandCapture.Clear();
        await _job.ExecuteAsync(args);

        using (CurrentTenant.Change(tenantId))
        {
            var detail = await _appService.GetAsync(runId);
            detail.StatusCode.ShouldBe(CheckRunStatusCodes.Completed);
            detail.Findings.Items.Count(item => item.KindCode == DifferenceKindCodes.EndpointRemoved).ShouldBe(1);
        }

        string.Join(Environment.NewLine, _commandCapture.Commands)
            .ShouldNotContain("UPDATE", Case.Insensitive);
    }

    // Tamamlanan kosunun durum olayinin bulgu ozetini tasidigini ve tuketiciyi ek sayfa okumasindan kurtardigini kanitlar.
    [Fact]
    public async Task Completed_Run_Event_Should_Carry_Finding_Summary()
    {
        var tenantId = Guid.NewGuid();
        var fixture = await CreateSnapshotGraphAsync(tenantId, BaseSpec, TargetSpec);
        var runId = await PrepareAsync(tenantId, fixture.BaseSnapshotId, fixture.TargetSnapshotId);
        var published = new List<ContractCheckRunStatusChangedEto>();
        using var subscription = GetRequiredService<ILocalEventBus>()
            .Subscribe<ContractCheckRunStatusChangedEto>(eto =>
            {
                published.Add(eto);
                return Task.CompletedTask;
            });

        await _job.ExecuteAsync(new ContractCheckExecutionJobArgs { RunId = runId, TenantId = tenantId });

        var completed = published.Last(eto => eto.StatusCode == CheckRunStatusCodes.Completed);
        completed.RunId.ShouldBe(runId);
        completed.TenantId.ShouldBe(tenantId);
        completed.MaxSeverityCode.ShouldBe(DifferenceSeverityCodes.Breaking);

        using (CurrentTenant.Change(tenantId))
        {
            var detail = await _appService.GetAsync(runId);
            completed.NewFindingCount.ShouldBe(detail.Findings.Items.Count);
        }
    }

    // Bulgusuz gecisin ozet alanlarini bos biraktigini ve repository'ye gitmedigini kanitlar.
    [Fact]
    public async Task Running_Transition_Event_Should_Carry_An_Empty_Finding_Summary()
    {
        var tenantId = Guid.NewGuid();
        var fixture = await CreateSnapshotGraphAsync(tenantId, BaseSpec, TargetSpec);
        var runId = await PrepareAsync(tenantId, fixture.BaseSnapshotId, fixture.TargetSnapshotId);
        var published = new List<ContractCheckRunStatusChangedEto>();
        using var subscription = GetRequiredService<ILocalEventBus>()
            .Subscribe<ContractCheckRunStatusChangedEto>(eto =>
            {
                published.Add(eto);
                return Task.CompletedTask;
            });

        using (CurrentTenant.Change(tenantId))
        {
            await WithUnitOfWorkAsync(() => _executionManager.StartAsync(runId));
        }

        var running = published.Last(eto => eto.StatusCode == CheckRunStatusCodes.Running);
        running.NewFindingCount.ShouldBe(0);
        running.MaxSeverityCode.ShouldBeNull();
    }

    // BuildContext sonrasi source tanimi degisse bile execution'in donmus snapshot govdelerini kullandigini kanitlar.
    [Fact]
    public async Task Execution_Should_Keep_Its_Context_When_Source_Definition_Changes()
    {
        var tenantId = Guid.NewGuid();
        var fixture = await CreateSnapshotGraphAsync(tenantId, BaseSpec, TargetSpec);
        var runId = await PrepareAsync(tenantId, fixture.BaseSnapshotId, fixture.TargetSnapshotId);
        ContractCheckExecutionContextModel context;

        using (CurrentTenant.Change(tenantId))
        {
            await WithUnitOfWorkAsync(() => _executionManager.StartAsync(runId));
            context = await WithUnitOfWorkAsync(
                () => _executionManager.BuildContextAsync(runId, [], ignoreInternal: false));
            await WithUnitOfWorkAsync(async () =>
            {
                var source = await SourceRepository.GetAsync(fixture.SourceId);
                SourceManager.Update(source, new UpdateSpecSourceModel
                {
                    Name = "changed-source",
                    BaseUrl = "https://changed.test"
                });
                await SourceRepository.UpdateAsync(source, autoSave: true);
            });

            var result = await _executionManager.ExecuteAsync(context);
            await WithUnitOfWorkAsync(() => _executionManager.CompleteAsync(result));
            var detail = await _appService.GetAsync(runId);

            detail.Findings.Items.ShouldContain(item => item.KindCode == DifferenceKindCodes.EndpointRemoved);
            detail.StatusCode.ShouldBe(CheckRunStatusCodes.Completed);
        }
    }

    // Parse hatasinin kararli kodla Failed yazildigini ve ABP retry icin ozgun hatanin yutulmadigini kanitlar.
    [Fact]
    public async Task Failed_Execution_Should_Leave_Failed_Run_Instead_Of_Running()
    {
        var tenantId = Guid.NewGuid();
        var fixture = await CreateSnapshotGraphAsync(tenantId, BaseSpec, InvalidSpec);
        var runId = await PrepareAsync(tenantId, fixture.BaseSnapshotId, fixture.TargetSnapshotId);

        await Should.ThrowAsync<BusinessException>(() => _job.ExecuteAsync(
            new ContractCheckExecutionJobArgs { RunId = runId, TenantId = tenantId }));

        using (CurrentTenant.Change(tenantId))
        {
            var status = await _appService.GetStatusAsync(runId);
            status.StatusCode.ShouldBe(CheckRunStatusCodes.Failed);
            status.ErrorMessage.ShouldBe(SpecSnapshotExceptionCodes.ParseFailed);
            status.CompletedAt.ShouldNotBeNull();
        }
    }

    // Yeni tetiklemenin esigi asmis Running run'i zamanlayici olmadan Failed terminal durumuna getirdigini kanitlar.
    [Fact]
    public async Task Next_Prepare_Should_Recover_Stale_Running_Run()
    {
        var tenantId = Guid.NewGuid();
        var fixture = await CreateSnapshotGraphAsync(tenantId, BaseSpec, TargetSpec);
        var staleRunId = await CreateStaleRunningAsync(tenantId, fixture.BaseSnapshotId, fixture.TargetSnapshotId);

        await PrepareAsync(tenantId, fixture.BaseSnapshotId, fixture.TargetSnapshotId);

        using (CurrentTenant.Change(tenantId))
        {
            var status = await _appService.GetStatusAsync(staleRunId);
            status.StatusCode.ShouldBe(CheckRunStatusCodes.Failed);
            status.ErrorMessage.ShouldBe(ContractCheckRunExceptionCodes.StaleRunningRecovered);
        }
    }

    // Baska tenant snapshot kimlikleriyle tetiklemenin NotFound oldugunu ve run acmadigini kanitlar.
    [Fact]
    public async Task Other_Tenant_Snapshots_Should_Not_Be_Triggerable()
    {
        var ownerTenantId = Guid.NewGuid();
        var callerTenantId = Guid.NewGuid();
        var fixture = await CreateSnapshotGraphAsync(ownerTenantId, BaseSpec, TargetSpec);

        using (CurrentTenant.Change(callerTenantId))
        {
            var exception = await Should.ThrowAsync<BusinessException>(() => _appService.ExecuteAsync(
                new ExecuteContractCheckDto
                {
                    BaseSnapshotId = fixture.BaseSnapshotId,
                    TargetSnapshotId = fixture.TargetSnapshotId
                }));
            exception.Code.ShouldBe(ContractCheckRunExceptionCodes.SnapshotNotFound);
            (await _runRepository.GetCountAsync()).ShouldBe(0);
        }
    }

    // Runtime scope tiplerinin EF modeline entity, property veya owned JSON olarak girmedigini kanitlar.
    [Fact]
    public async Task Scope_Rules_Should_Not_Be_Persisted_In_Any_Table()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var dbContext = await GetRequiredService<IDbContextProvider<ApiContractCheckerDbContext>>()
                .GetDbContextAsync();
            var runType = dbContext.Model.FindEntityType(typeof(ContractCheckRun));

            runType.ShouldNotBeNull();
            runType.GetProperties().ShouldNotContain(property =>
                property.Name.Contains("Scope", StringComparison.Ordinal));
            dbContext.Model.GetEntityTypes().ShouldNotContain(entityType =>
                entityType.ClrType == typeof(ContractCheckScopeRuleModel));
        });
    }

    // Tenant icinde snapshot ciftini dogrulayip Pending run'i kisa UOW'de yazar.
    private async Task<Guid> PrepareAsync(Guid tenantId, Guid baseSnapshotId, Guid targetSnapshotId)
    {
        using (CurrentTenant.Change(tenantId))
        {
            var run = await WithUnitOfWorkAsync(
                () => _executionManager.PrepareAsync(baseSnapshotId, targetSnapshotId));
            return run.Id;
        }
    }

    // Esigi asmis Running run'i entity gecisleriyle test verisi olarak kurar.
    private async Task<Guid> CreateStaleRunningAsync(
        Guid tenantId,
        Guid baseSnapshotId,
        Guid targetSnapshotId)
    {
        using (CurrentTenant.Change(tenantId))
        {
            return await WithUnitOfWorkAsync(async () =>
            {
                var running = await _statusRepository.FirstAsync(status => status.Code == CheckRunStatusCodes.Running);
                var run = await _runManager.CreateAsync(Guid.NewGuid(), baseSnapshotId, targetSnapshotId, tenantId);
                _runManager.Start(run, running.Id, DateTime.UtcNow.AddHours(-2));
                await _runRepository.InsertAsync(run, autoSave: true);
                return run.Id;
            });
        }
    }
}
