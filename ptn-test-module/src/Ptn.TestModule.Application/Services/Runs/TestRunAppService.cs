using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using FluentValidation;
using Ptn.TestModule.BackgroundJobs.Runs;
using Ptn.TestModule.Dtos.Runs;
using Ptn.TestModule.Interface.Runs;
using Ptn.TestModule.Managers.Runs;
using Ptn.TestModule.Mappers.Runs;
using Ptn.TestModule.Models.Runs;
using Ptn.TestModule.Permissions;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Threading;
using Ptn.ApiContractChecker.Services.Snapshots;

namespace Ptn.TestModule.Services.Runs;

// islevi: Kosum olusturma, okuma, idempotent claim ve atomik terminal yazimi orkestre eder.
// sistemdeki gorevi: Controller ile Manager/Repository katmanlarini Mapperly ve permission kapilariyla baglar.
/// <summary>Test kosumu use-case'lerinin Application uygulamasidir.</summary>
[RemoteService(IsEnabled = false)]
public class TestRunAppService : TestModuleAppService, ITestRunAppService
{
    /// <summary>Kosum dikeyinin saf katmanlar-arasi eslemelerini yapar.</summary>
    private static readonly TestRunMapper Mapper = new();

    /// <summary>Kosum yasam dongusu mutasyonlarini sahiplenen Manager'dir.</summary>
    private readonly TestRunManager _testRunManager;

    /// <summary>Terminal sonuc invariantlarini ve aggregate kurulumunu sahiplenen Manager'dir.</summary>
    private readonly TestRunResultManager _testRunResultManager;

    /// <summary>Tenant ayarindan ortam snapshot'i cozen Manager'dir.</summary>
    private readonly RunEnvironmentBindingManager _environmentBindingManager;

    /// <summary>Kuru kosum celiski bildirimini sahiplenen Manager'dir.</summary>
    private readonly DryRunContradictionManager _dryRunContradictionManager;

    /// <summary>TestRun aggregate kalicilik siniridir.</summary>
    private readonly ITestRunRepository _testRunRepository;

    /// <summary>TestRunResult aggregate kalicilik ve bulgulu okuma siniridir.</summary>
    private readonly ITestRunResultRepository _testRunResultRepository;

    /// <summary>Aktif ABP istek iptal token'ini saglayan provider'dir.</summary>
    private readonly ICancellationTokenProvider _cancellationTokenProvider;

    /// <summary>Dayanikli kosum icra job'ini kuyruga veren ABP job manager'idir.</summary>
    private readonly IBackgroundJobManager _backgroundJobManager;
    private readonly IValidator<TestRunListInput> _listValidator;
    private readonly IRunArtifactStore _artifactStore;
    private readonly IHarArtifactStore _harArtifactStore;
    private readonly RunCancellationManager _runCancellationManager;

    /// <summary>Webhook sir dogrulamasini ve senaryo cozumunu sahiplenen Manager'dir.</summary>
    private readonly RunWebhookManager _runWebhookManager;

    /// <summary>Otomatik tetikleyicilerin idempotency kararini sahiplenen Manager'dir.</summary>
    private readonly AutomatedRunTriggerManager _automatedRunTriggerManager;
    private readonly IValidator<WebhookTestRunDto> _webhookValidator;
    private readonly ISpecSnapshotAppService _specSnapshotAppService;

    /// <summary>Application orkestrasyonunu Manager ve repository bagimliliklariyla kurar.</summary>
    public TestRunAppService(
        TestRunManager testRunManager,
        TestRunResultManager testRunResultManager,
        RunEnvironmentBindingManager environmentBindingManager,
        DryRunContradictionManager dryRunContradictionManager,
        ITestRunRepository testRunRepository,
        ITestRunResultRepository testRunResultRepository,
        ICancellationTokenProvider cancellationTokenProvider,
        IBackgroundJobManager backgroundJobManager,
        IValidator<TestRunListInput> listValidator,
        IRunArtifactStore artifactStore,
        IHarArtifactStore harArtifactStore,
        RunCancellationManager runCancellationManager,
        RunWebhookManager runWebhookManager,
        AutomatedRunTriggerManager automatedRunTriggerManager,
        IValidator<WebhookTestRunDto> webhookValidator,
        ISpecSnapshotAppService specSnapshotAppService)
    {
        _runWebhookManager = runWebhookManager;
        _automatedRunTriggerManager = automatedRunTriggerManager;
        _webhookValidator = webhookValidator;
        _testRunManager = testRunManager;
        _testRunResultManager = testRunResultManager;
        _environmentBindingManager = environmentBindingManager;
        _dryRunContradictionManager = dryRunContradictionManager;
        _testRunRepository = testRunRepository;
        _testRunResultRepository = testRunResultRepository;
        _cancellationTokenProvider = cancellationTokenProvider;
        _backgroundJobManager = backgroundJobManager;
        _listValidator = listValidator;
        _artifactStore = artifactStore;
        _harArtifactStore = harArtifactStore;
        _runCancellationManager = runCancellationManager;
        _specSnapshotAppService = specSnapshotAppService;
    }

    /// <summary>Kimligi verilen test kosumunu permission kontrolunden sonra getirir.</summary>
    public async Task<TestRunDto> GetAsync(Guid id)
    {
        await CheckPolicyAsync(TestModulePermissions.Runs.View);
        var entity = await _testRunManager.EnsureExistsAsync(
            id,
            cancellationToken: _cancellationTokenProvider.Token);
        return Mapper.Map(entity);
    }

    /// <summary>Kosumlari kararli siralamayla sayfalar; agir rapor kolonlari bu uca girmez.</summary>
    public async Task<PagedResultDto<TestRunHeaderDto>> GetListAsync(TestRunListInput input)
    {
        await CheckPolicyAsync(TestModulePermissions.Runs.View);
        await _listValidator.ValidateAndThrowAsync(input, _cancellationTokenProvider.Token);
        var page = await _testRunRepository.GetHeaderPageAsync(Mapper.Map(input), _cancellationTokenProvider.Token);
        return new PagedResultDto<TestRunHeaderDto>(page.TotalCount, Mapper.Map(new List<TestRunHeader>(page.Items)));
    }

    /// <summary>Running kosuma kooperatif iptal talebi yazar.</summary>
    public async Task CancelAsync(Guid id)
    {
        await CheckPolicyAsync(TestModulePermissions.Runs.Cancel);
        var token = _cancellationTokenProvider.Token;
        var entity = await _testRunManager.EnsureExistsAsync(id, cancellationToken: token);
        await _runCancellationManager.RequestAsync(entity, token);
        await _testRunRepository.UpdateAsync(entity, autoSave: true, cancellationToken: token);
    }

    /// <summary>Terminal sonuc artefaktini format koduyla okuyup public metin govdesine cevirir.</summary>
    public async Task<RunArtifactContentDto> GetArtifactContentAsync(Guid id, string format)
    {
        await CheckPolicyAsync(TestModulePermissions.Runs.View);
        var descriptor = TestRunManager.CreateArtifactDescriptor(
            await _testRunResultManager.EnsureExistsAsync(id, cancellationToken: _cancellationTokenProvider.Token),
            format);
        var content = TestRunManager.EnsureArtifactContent(
            await _artifactStore.ReadAsync(descriptor.BlobName, _cancellationTokenProvider.Token));
        return Mapper.Map(new RunArtifactContent
        {
            Format = descriptor.Format,
            BlobName = descriptor.BlobName,
            ContentType = descriptor.ContentType,
            Content = content
        });
    }

    /// <summary>Kosumun HAR artefaktini okuyup public metin govdesine cevirir.</summary>
    public async Task<RunArtifactContentDto> GetHarContentAsync(Guid id)
    {
        await CheckPolicyAsync(TestModulePermissions.Runs.View);
        var run = await _testRunManager.EnsureExistsAsync(id, cancellationToken: _cancellationTokenProvider.Token);
        var descriptor = TestRunManager.CreateHarDescriptor(run);
        var content = TestRunManager.EnsureArtifactContent(
            await _harArtifactStore.ReadAsync(descriptor.BlobName, _cancellationTokenProvider.Token));
        return Mapper.Map(new RunArtifactContent
        {
            Format = descriptor.Format,
            BlobName = descriptor.BlobName,
            ContentType = descriptor.ContentType,
            Content = content
        });
    }

    /// <summary>Kosumu terminal hukmu, bulgulari ve teshis raporuyla birlikte getirir.</summary>
    public async Task<TestReportDetailDto> GetReportAsync(Guid id)
    {
        await CheckPolicyAsync(TestModulePermissions.Runs.View);
        var report = await _testRunRepository.GetReportAsync(id, _cancellationTokenProvider.Token);
        return Mapper.Map(TestRunResultManager.MarkHealing(_testRunResultManager.EnsureReportFound(report, id)));
    }

    /// <summary>Kuru kosum gozlemini sozlesme hukmunu degistirmeyen celiski bildirimine cevirir.</summary>
    public async Task<DryRunContradictionReportDto> GetDryRunContradictionAsync(Guid id)
    {
        await CheckPolicyAsync(TestModulePermissions.Runs.View);
        var report = await _testRunRepository.GetReportAsync(id, _cancellationTokenProvider.Token);
        return Mapper.Map(_dryRunContradictionManager.Create(
            _testRunResultManager.EnsureReportFound(report, id)));
    }

    /// <summary>Terminal sonucun ihracat artefakt baglarini getirir; agir cikti satirda tutulmaz.</summary>
    public async Task<RunArtifactLinksDto> GetArtifactLinksAsync(Guid id)
    {
        await CheckPolicyAsync(TestModulePermissions.Runs.View);
        var entity = await _testRunResultManager.EnsureExistsAsync(
            id,
            cancellationToken: _cancellationTokenProvider.Token);
        return Mapper.Map(TestRunResultManager.ReadArtifactLinks(entity));
    }

    /// <summary>Kimligi verilen terminal sonucu tum bulgulariyla getirir.</summary>
    public async Task<TestRunResultDto> GetResultAsync(Guid id)
    {
        await CheckPolicyAsync(TestModulePermissions.Runs.View);
        var entity = await _testRunResultRepository.GetWithFindingsAsync(
            id,
            _cancellationTokenProvider.Token);
        return Mapper.Map(_testRunResultManager.EnsureFound(entity, id));
    }

    /// <summary>Tenant ortam ayarini snapshot'layip yeni Pending kosumu kalicilastirir.</summary>
    public async Task<TestRunDto> CreateAsync(CreateTestRunDto input)
    {
        await CheckPolicyAsync(TestModulePermissions.Runs.Trigger);
        var cancellationToken = _cancellationTokenProvider.Token;
        var binding = await _environmentBindingManager.ResolveAsync(input.EnvironmentKey, cancellationToken);
        
        string? specFingerprint = null;
        if (binding.SpecSnapshotId != Guid.Empty)
        {
            var snapshot = await _specSnapshotAppService.GetAsync(binding.SpecSnapshotId);
            specFingerprint = snapshot?.SpecContent?.CanonicalHash;
        }

        var entity = await _testRunManager.CreateAsync(
            Mapper.Map(input),
            binding,
            input.CanonicalInputs,
            specFingerprint,
            input.DbSchemaFingerprint,
            input.RunnerRef,
            cancellationToken);
        var saved = await _testRunRepository.InsertAsync(
            entity,
            autoSave: true,
            cancellationToken: cancellationToken);
        return Mapper.Map(saved);
    }

    /// <summary>Pending kosumu olusturup dayanikli icra job'ini kuyruga verir.</summary>
    public async Task<TestRunDto> TriggerAsync(CreateTestRunDto input)
    {
        var created = await CreateAsync(input);
        await _backgroundJobManager.EnqueueAsync(new ExecuteTestRunArgs
        {
            TestRunId = created.Id,
            TenantId = CurrentTenant.Id,
            TraceId = created.TraceId ?? string.Empty
        });
        return created;
    }

    /// <summary>Paylasilan sirla dogrulanmis webhook cagrisini idempotent bicimde kosuma cevirir.</summary>
    public async Task<TestRunDto> TriggerByWebhookAsync(string? secret, WebhookTestRunDto input)
    {
        var cancellationToken = _cancellationTokenProvider.Token;
        await _runWebhookManager.EnsureAuthorizedAsync(secret, cancellationToken);
        await _webhookValidator.ValidateAndThrowAsync(input, cancellationToken);
        var request = await _runWebhookManager.CreateRunRequestAsync(Mapper.Map(input), cancellationToken);
        var outcome = await _automatedRunTriggerManager.ResolveAsync(request, cancellationToken);
        if (!outcome.IsNew)
        {
            return Mapper.Map(outcome.Run);
        }

        var saved = await _testRunRepository.InsertAsync(
            outcome.Run,
            autoSave: true,
            cancellationToken: cancellationToken);
        await _backgroundJobManager.EnqueueAsync(new ExecuteTestRunArgs
        {
            TestRunId = saved.Id,
            TenantId = saved.TenantId,
            TraceId = saved.TraceId ?? string.Empty
        });
        return Mapper.Map(saved);
    }

    /// <summary>Pending kosumu idempotent bicimde Running durumuna claim edip guncel kaydi dondurur.</summary>
    public async Task<TestRunClaimDto> StartAsync(Guid id)
    {
        await CheckPolicyAsync(TestModulePermissions.Runs.Start);
        var cancellationToken = _cancellationTokenProvider.Token;
        var entity = await _testRunManager.EnsureExistsAsync(id, cancellationToken: cancellationToken);
        var claimed = await _testRunManager.StartAsync(entity, Clock.Now, cancellationToken);
        if (claimed)
        {
            await _testRunRepository.UpdateAsync(entity, autoSave: true, cancellationToken: cancellationToken);
        }
        return Mapper.Map(new TestRunClaimResult
        {
            Claimed = claimed,
            Run = entity
        });
    }

    /// <summary>Running kosum ile yeni terminal sonucu ayni Application UoW icinde kalicilastirir.</summary>
    public async Task<TestRunResultDto> WriteTerminalAsync(Guid id, WriteTestRunTerminalDto input)
    {
        await CheckPolicyAsync(TestModulePermissions.Runs.WriteResult);
        var cancellationToken = _cancellationTokenProvider.Token;
        var testRun = await _testRunManager.EnsureExistsAsync(id, cancellationToken: cancellationToken);
        var result = await _testRunResultManager.WriteAsync(
            testRun,
            Mapper.Map(input),
            input.DurationMs,
            Clock.Now,
            input.HarBlobName, cancellationToken);

        await _testRunRepository.UpdateAsync(testRun, cancellationToken: cancellationToken);
        var saved = await _testRunResultRepository.InsertAsync(
            result,
            autoSave: true,
            cancellationToken: cancellationToken);
        return Mapper.Map(saved);
    }
}
