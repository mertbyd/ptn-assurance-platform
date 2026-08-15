using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Nexum.Abp.Foundation.Managers;
using Nexum.Abp.Foundation.Querying;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.Constants.Runs.Lookups;
using Ptn.TestModule.Entities.Lookups;
using Ptn.TestModule.Entities.Runs;
using Ptn.TestModule.ExceptionCodes.Runs;
using Ptn.TestModule.Interface.Lookups;
using Ptn.TestModule.Interface.Runs;
using Ptn.TestModule.Models.Runs;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace Ptn.TestModule.Managers.Runs;

// islevi: Pending kosum olusturma, idempotent claim, terminal gecis, stale recovery ve silme kurallarini yonetir.
// sistemdeki gorevi: TestRun veri kabugunun tum yasam dongusu mutasyonlarini tek domain sahibinde tutar.
/// <summary>
/// Test kosum aggregate'inin kimlik uretimi ve durum gecislerini yonetir.
/// </summary>
public class TestRunManager : FoundationManager<TestRun, Guid>
{
    /// <summary>Kosum aggregate sorgularini ve kalicilik sinirini saglayan repository'dir.</summary>
    private readonly ITestRunRepository _repository;

    /// <summary>Kosum motoru durum kodlarini kimlige cozen lookup repository'sidir.</summary>
    private readonly ITestRunStatusRepository _statusRepository;

    /// <summary>Tetikleyici turu kodlarini kimlige cozen lookup repository'sidir.</summary>
    private readonly ITestTriggerKindRepository _triggerKindRepository;

    /// <summary>Foundation tekillik kapisinin bu aggregate icin kullandigi kararli hata kodudur.</summary>
    protected override string AlreadyExistsErrorCode => TestModuleRunErrorCodes.RunAlreadyClaimed;

    // Kosum ve iki lookup deposunu yasam dongusu kurallarina baglar.
    /// <summary>Test kosum manager'ini repository ve lookup bagimliliklariyla kurar.</summary>
    public TestRunManager(
        ITestRunRepository repository,
        ITestRunStatusRepository statusRepository,
        ITestTriggerKindRepository triggerKindRepository)
        : base(repository)
    {
        _repository = repository;
        _statusRepository = statusRepository;
        _triggerKindRepository = triggerKindRepository;
    }

    // Dogrulanmis ortam snapshot'i ile ilk kisa UoW'da yazilacak Pending kaydi kurar.
    /// <summary>Yeni Pending kosum kaydini trace ve history kimlikleriyle olusturur.</summary>
    public async Task<TestRun> CreateAsync(
        TestRunCreateModel model,
        TestRunEnvironmentBinding environmentBinding,
        string canonicalInputs,
        string? specFingerprint,
        string? dbSchemaFingerprint,
        string? runnerRef,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(environmentBinding);
        var normalizedModel = Normalize(model);
        var normalizedBinding = Normalize(environmentBinding);
        var pendingStatusId = await GetStatusIdAsync(TestRunStatusCodes.Pending, cancellationToken);
        var triggerKindId = await GetTriggerKindIdAsync(normalizedModel.TriggerKindCode, cancellationToken);

        return new TestRun(
            GuidGenerator.Create(),
            pendingStatusId,
            triggerKindId,
            CurrentTenant.Id,
            normalizedModel,
            normalizedBinding,
            ComputeHistoryId(normalizedModel.TestKey, normalizedBinding.EnvironmentKey, canonicalInputs),
            CreateTraceId(),
            NormalizeFingerprint(specFingerprint),
            NormalizeFingerprint(dbSchemaFingerprint),
            NormalizeOptionalText(runnerRef, nameof(runnerRef), TestRunConsts.MaxRunnerRefLength));
    }

    // Ikinci teslimi exception yerine no-op yapan Pending -> Running claim gecisini uygular.
    /// <summary>Pending kosumu Running durumuna claim eder; zaten claim edilmisse false doner.</summary>
    public async Task<bool> StartAsync(
        TestRun entity,
        DateTime startedAt,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        var pendingStatusId = await GetStatusIdAsync(TestRunStatusCodes.Pending, cancellationToken);
        if (entity.RunStatusId != pendingStatusId)
        {
            return false;
        }

        entity.RunStatusId = await GetStatusIdAsync(TestRunStatusCodes.Running, cancellationToken);
        entity.StartedAt = startedAt;
        return true;
    }

    // Running kosumu istenen terminal motor durumuna tasir ve artefakt snapshot'ini baglar.
    /// <summary>Kosum durumunu terminal lookup koduna gecirip tamamlanma alanlarini atar.</summary>
    public async Task<TestRun> CompleteAsync(
        TestRun entity,
        string terminalStatusCode,
        DateTime completedAt,
        string? harBlobName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        var runningStatusId = await GetStatusIdAsync(TestRunStatusCodes.Running, cancellationToken);
        if (entity.RunStatusId != runningStatusId)
        {
            throw new BusinessException(TestModuleRunErrorCodes.RunAlreadyClaimed);
        }

        entity.RunStatusId = await GetStatusIdAsync(terminalStatusCode, cancellationToken);
        entity.CompletedAt = completedAt;
        entity.HarBlobName = NormalizeOptionalText(
            harBlobName,
            nameof(harBlobName),
            TestRunConsts.MaxHarBlobNameLength);
        return entity;
    }

    // Asili Running kosularini tek sorguda alip Aborted durumuna toplu olarak tasir.
    /// <summary>Esik zamandan once baslamis Running kosumlari Aborted durumuna kurtarir.</summary>
    public async Task<int> RecoverStaleRunningAsync(
        DateTime startedBefore,
        DateTime recoveredAt,
        CancellationToken cancellationToken = default)
    {
        var runningStatusId = await GetStatusIdAsync(TestRunStatusCodes.Running, cancellationToken);
        var abortedStatusId = await GetStatusIdAsync(TestRunStatusCodes.Aborted, cancellationToken);
        var staleRuns = await _repository.GetStaleRunningAsync(
            runningStatusId,
            startedBefore,
            cancellationToken);

        foreach (var staleRun in staleRuns)
        {
            staleRun.RunStatusId = abortedStatusId;
            staleRun.CompletedAt = recoveredAt;
        }

        return staleRuns.Count;
    }

    // Kosum kayitlari tarihsel oldugu icin delete use-case'ini reddeder.
    /// <summary>Test kosum kaydinin silinmesine her durumda izin vermez.</summary>
    public Task EnsureCanDeleteAsync(
        TestRun entity,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        throw new BusinessException(TestModuleRunErrorCodes.RunDeletionNotAllowed);
    }

    // W3C Activity API'sinden 16 baytlik kucuk harfli trace kimligi uretir.
    /// <summary>32 karakterlik kucuk harf hex W3C trace-id uretir.</summary>
    public static string CreateTraceId()
    {
        var traceId = ActivityTraceId.CreateRandom().ToHexString();
        if (!Regex.IsMatch(traceId, TestRunConsts.TraceIdPattern))
        {
            throw new BusinessException(TestModuleRunErrorCodes.InvalidTraceId);
        }

        return traceId;
    }

    // Test, ortam ve onceden kanoniklestirilmis girdileri FIPS uyumlu SHA-256 ile muhurlar.
    /// <summary>Trend kovasi icin kararli SHA-256 history kimligi hesaplar.</summary>
    public static string ComputeHistoryId(
        string testKey,
        string environmentKey,
        string canonicalInputs)
    {
        var canonical = string.Join("¦", testKey, environmentKey, canonicalInputs);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)))
            .ToLowerInvariant();
    }

    // Create girdisinin metin alanlarini DBML uzunluklarina gore kanoniklestirir.
    /// <summary>Yeni kosum modelinin metin alanlarini dogrulayip normalize eder.</summary>
    private static TestRunCreateModel Normalize(TestRunCreateModel model)
    {
        return new TestRunCreateModel
        {
            ScenarioId = model.ScenarioId,
            TestKey = NormalizeRequiredText(
                model.TestKey,
                nameof(model.TestKey),
                TestRunConsts.MaxTestKeyLength),
            TriggerKindCode = NormalizeRequiredText(
                model.TriggerKindCode,
                nameof(model.TriggerKindCode),
                TestResultFindingConsts.MaxKindCodeLength),
            TriggerRef = NormalizeOptionalText(
                model.TriggerRef,
                nameof(model.TriggerRef),
                TestRunConsts.MaxTriggerRefLength),
            IsDryRun = model.IsDryRun
        };
    }

    // Cozulmus ortam baglamasinin snapshot alanlarini kanoniklestirir.
    /// <summary>Ortam baglamasinin zorunlu alanlarini DBML sinirlarinda normalize eder.</summary>
    private static TestRunEnvironmentBinding Normalize(TestRunEnvironmentBinding binding)
    {
        return new TestRunEnvironmentBinding
        {
            EnvironmentKey = NormalizeRequiredText(
                binding.EnvironmentKey,
                nameof(binding.EnvironmentKey),
                TestRunConsts.MaxEnvironmentKeyLength),
            BaseUrl = binding.BaseUrl,
            SpecSnapshotId = binding.SpecSnapshotId,
            DbConnectionId = binding.DbConnectionId,
            SecretRef = binding.SecretRef
        };
    }

    // Opsiyonel checker fingerprint'ini 64 karakterlik kucuk harfli snapshot'a cevirir.
    /// <summary>Opsiyonel fingerprint degerini kucuk harfli 64 karakter bicimine getirir.</summary>
    private static string? NormalizeFingerprint(string? fingerprint)
    {
        if (string.IsNullOrWhiteSpace(fingerprint))
        {
            return null;
        }

        var normalized = fingerprint.Trim().ToLowerInvariant();
        if (!Regex.IsMatch(normalized, TestRunConsts.FingerprintPattern))
        {
            throw new BusinessException(TestModuleRunErrorCodes.InvalidFingerprint)
                .WithData(nameof(fingerprint), fingerprint);
        }

        return normalized;
    }

    // Kosum durumu lookup kodunu aggregate'e yazilacak kimlige cozer.
    /// <summary>Verilen kosum durumu kodunun lookup kimligini getirir.</summary>
    private async Task<Guid> GetStatusIdAsync(string statusCode, CancellationToken cancellationToken)
    {
        var status = await _statusRepository.FindAsync(
            new RepositoryQuery<TestRunStatus>().Where(item => item.Code == statusCode),
            cancellationToken);
        return status?.Id ?? throw new EntityNotFoundException(typeof(TestRunStatus));
    }

    // Tetikleyici lookup kodunu aggregate'e yazilacak kimlige cozer.
    /// <summary>Verilen tetikleyici turu kodunun lookup kimligini getirir.</summary>
    private async Task<Guid> GetTriggerKindIdAsync(
        string triggerKindCode,
        CancellationToken cancellationToken)
    {
        var triggerKind = await _triggerKindRepository.FindAsync(
            new RepositoryQuery<TestTriggerKind>().Where(item => item.Code == triggerKindCode),
            cancellationToken);
        return triggerKind?.Id ?? throw new EntityNotFoundException(typeof(TestTriggerKind));
    }
}
