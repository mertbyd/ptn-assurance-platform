using System;
using System.Threading.Tasks;
using Ptn.TestModule.BackgroundJobs.Runs;
using Ptn.TestModule.Dtos.Runs;
using Ptn.TestModule.Interface.Runs;
using Ptn.TestModule.Managers.Runs;
using Ptn.TestModule.Mappers.Runs;
using Ptn.TestModule.Models.Runs;
using Ptn.TestModule.Permissions;
using Volo.Abp;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Threading;

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

    /// <summary>TestRun aggregate kalicilik siniridir.</summary>
    private readonly ITestRunRepository _testRunRepository;

    /// <summary>TestRunResult aggregate kalicilik ve bulgulu okuma siniridir.</summary>
    private readonly ITestRunResultRepository _testRunResultRepository;

    /// <summary>Aktif ABP istek iptal token'ini saglayan provider'dir.</summary>
    private readonly ICancellationTokenProvider _cancellationTokenProvider;

    /// <summary>Dayanikli kosum icra job'ini kuyruga veren ABP job manager'idir.</summary>
    private readonly IBackgroundJobManager _backgroundJobManager;

    /// <summary>Application orkestrasyonunu Manager ve repository bagimliliklariyla kurar.</summary>
    public TestRunAppService(
        TestRunManager testRunManager,
        TestRunResultManager testRunResultManager,
        RunEnvironmentBindingManager environmentBindingManager,
        ITestRunRepository testRunRepository,
        ITestRunResultRepository testRunResultRepository,
        ICancellationTokenProvider cancellationTokenProvider,
        IBackgroundJobManager backgroundJobManager)
    {
        _testRunManager = testRunManager;
        _testRunResultManager = testRunResultManager;
        _environmentBindingManager = environmentBindingManager;
        _testRunRepository = testRunRepository;
        _testRunResultRepository = testRunResultRepository;
        _cancellationTokenProvider = cancellationTokenProvider;
        _backgroundJobManager = backgroundJobManager;
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

    /// <summary>Kimligi verilen terminal sonucu tum bulgulariyla getirir.</summary>
    public async Task<TestRunResultDto> GetResultAsync(Guid id)
    {
        await CheckPolicyAsync(TestModulePermissions.Runs.View);
        var entity = await _testRunResultRepository.GetWithFindingsAsync(
            id,
            _cancellationTokenProvider.Token);
        return Mapper.Map(entity ?? throw new EntityNotFoundException(typeof(Ptn.TestModule.Entities.Runs.TestRunResult), id));
    }

    /// <summary>Tenant ortam ayarini snapshot'layip yeni Pending kosumu kalicilastirir.</summary>
    public async Task<TestRunDto> CreateAsync(CreateTestRunDto input)
    {
        await CheckPolicyAsync(TestModulePermissions.Runs.Trigger);
        var cancellationToken = _cancellationTokenProvider.Token;
        var binding = await _environmentBindingManager.ResolveAsync(input.EnvironmentKey, cancellationToken);
        var entity = await _testRunManager.CreateAsync(
            Mapper.Map(input),
            binding,
            input.CanonicalInputs,
            input.SpecFingerprint,
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
            input.HarBlobName,
            cancellationToken);

        await _testRunRepository.UpdateAsync(testRun, cancellationToken: cancellationToken);
        var saved = await _testRunResultRepository.InsertAsync(
            result,
            autoSave: true,
            cancellationToken: cancellationToken);
        return Mapper.Map(saved);
    }
}
