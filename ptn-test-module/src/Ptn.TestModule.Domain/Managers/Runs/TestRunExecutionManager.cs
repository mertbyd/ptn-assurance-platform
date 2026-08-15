using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.Entities.Catalog;
using Ptn.TestModule.ExceptionCodes.Runs;
using Ptn.TestModule.Interface.Catalog;
using Ptn.TestModule.Interface.Runs;
using Ptn.TestModule.Models.Runs;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Timing;

namespace Ptn.TestModule.Managers.Runs;

// islevi: Dayanikli kosumun claim, hazirlik ve terminal yazim adimlarini tek domain kapisinda toplar.
// sistemdeki gorevi: Job'in UoW sinirlarini yonetmesini saglar; her karar mevcut Manager'lara devredilir (ADR-0015 §B).
/// <summary>
/// Bir test kosumunun dayanikli icra yasam dongusunu yonetir.
/// </summary>
public class TestRunExecutionManager : TestModuleDomainService
{
    /// <summary>Kosum yasam dongusu gecislerini sahiplenen Manager'dir.</summary>
    private readonly TestRunManager _testRunManager;

    /// <summary>Terminal sonuc invariantlarini sahiplenen Manager'dir.</summary>
    private readonly TestRunResultManager _testRunResultManager;

    /// <summary>Tenant ayarindan ortam snapshot'i cozen Manager'dir.</summary>
    private readonly RunEnvironmentBindingManager _environmentBindingManager;

    /// <summary>Kosum ani malzeme kaymasini degerlendiren Manager'dir.</summary>
    private readonly RunMaterialDriftManager _materialDriftManager;

    /// <summary>Kosum belgesini olgulara cozen dis runner siniridir.</summary>
    private readonly IWorkflowRunnerPort _workflowRunnerPort;

    /// <summary>TestRun aggregate kalicilik siniridir.</summary>
    private readonly ITestRunRepository _testRunRepository;

    /// <summary>TestRunResult aggregate kalicilik siniridir.</summary>
    private readonly ITestRunResultRepository _testRunResultRepository;

    /// <summary>Senaryo aggregate kalicilik siniridir.</summary>
    private readonly ITestScenarioRepository _testScenarioRepository;

    /// <summary>Terminal zaman damgalarini ureten ABP saatidir.</summary>
    private readonly IClock _clock;

    /// <summary>Kosum span'lerini ve hukum olcumlerini yayan Manager'dir.</summary>
    private readonly RunTelemetryManager _runTelemetryManager;

    // Kosum yasam dongusunun tum domain sahiplerini tek icra kapisina baglar.
    /// <summary>Icra manager'ini Manager, repository ve saat bagimliliklariyla kurar.</summary>
    public TestRunExecutionManager(
        TestRunManager testRunManager,
        TestRunResultManager testRunResultManager,
        RunEnvironmentBindingManager environmentBindingManager,
        RunMaterialDriftManager materialDriftManager,
        IWorkflowRunnerPort workflowRunnerPort,
        ITestRunRepository testRunRepository,
        ITestRunResultRepository testRunResultRepository,
        ITestScenarioRepository testScenarioRepository,
        IClock clock,
        RunTelemetryManager runTelemetryManager)
    {
        _runTelemetryManager = runTelemetryManager;
        _testRunManager = testRunManager;
        _testRunResultManager = testRunResultManager;
        _environmentBindingManager = environmentBindingManager;
        _materialDriftManager = materialDriftManager;
        _workflowRunnerPort = workflowRunnerPort;
        _testRunRepository = testRunRepository;
        _testRunResultRepository = testRunResultRepository;
        _testScenarioRepository = testScenarioRepository;
        _clock = clock;
    }

    // Ikinci teslimi exception yerine no-op yapan Pending -> Running claim'ini kalicilastirir.
    /// <summary>Kosumu idempotent bicimde Running durumuna claim eder.</summary>
    public async Task<bool> ClaimAsync(Guid testRunId, CancellationToken cancellationToken = default)
    {
        var entity = await _testRunManager.EnsureExistsAsync(testRunId, cancellationToken: cancellationToken);
        var claimed = await _testRunManager.StartAsync(entity, _clock.Now, cancellationToken);
        if (claimed)
        {
            await _testRunRepository.UpdateAsync(entity, autoSave: true, cancellationToken: cancellationToken);
        }

        return claimed;
    }

    // Ortam, senaryo belgesi, malzeme muhru ve runner girdilerini tek UoW icinde toplar.
    /// <summary>UoW disinda icra edilecek kosumun tam baglamini kurar.</summary>
    public async Task<TestRunExecutionContext> PrepareAsync(
        Guid testRunId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _testRunManager.EnsureExistsAsync(testRunId, cancellationToken: cancellationToken);
        using var span = _runTelemetryManager.StartExecution(entity);
        var scenario = await GetScenarioAsync(entity.ScenarioId, cancellationToken);
        var binding = await _environmentBindingManager.ResolveAsync(entity.EnvironmentKey, cancellationToken);
        var document = EnsureDocumentExists(scenario);

        return new TestRunExecutionContext
        {
            TestRunId = entity.Id,
            CompiledDocument = document,
            DocumentFacts = _workflowRunnerPort.ReadDocumentFacts(document),
            Inputs = BuildInputs(binding, entity.TraceId),
            EnvironmentBinding = binding,
            MaterialDrift = EvaluateDrift(scenario, entity),
            TraceId = entity.TraceId ?? string.Empty,
            TenantId = entity.TenantId
        };
    }

    // Terminal hukmu ile kosum kaydini ayni yeni UoW icinde atomik olarak kalicilastirir.
    /// <summary>Kosumu terminal duruma gecirip sonucunu bulgulariyla yazar.</summary>
    public async Task WriteTerminalAsync(
        Guid testRunId,
        TestRunTerminalModel terminal,
        long durationMs,
        string? harBlobName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(terminal);
        var entity = await _testRunManager.EnsureExistsAsync(testRunId, cancellationToken: cancellationToken);
        using var span = _runTelemetryManager.StartJudgement(entity);
        var result = await _testRunResultManager.WriteAsync(
            entity,
            terminal,
            NormalizeDurationMs(durationMs),
            _clock.Now,
            harBlobName,
            cancellationToken);

        await _testRunRepository.UpdateAsync(entity, cancellationToken: cancellationToken);
        await _testRunResultRepository.InsertAsync(result, autoSave: true, cancellationToken: cancellationToken);
        _runTelemetryManager.RecordTerminal(entity, terminal);
    }

    // Esik dakikayi saat uzerinden mutlak zamana cevirip asili Running kosumlari kurtarir.
    /// <summary>Esik suresinden once baslamis Running kosumlari Aborted durumuna kurtarir.</summary>
    public Task<int> RecoverStaleAsync(int thresholdMinutes, CancellationToken cancellationToken = default)
    {
        var recoveredAt = _clock.Now;
        return _testRunManager.RecoverStaleRunningAsync(
            recoveredAt.AddMinutes(-thresholdMinutes),
            recoveredAt,
            cancellationToken);
    }

    // Surec sinirinin olctugu long sureyi kalici kaydin int butcesine tasiyabilir hale getirir.
    /// <summary>Olculen kosum suresini kalici sonuc alaninin araligina indirger.</summary>
    private static int NormalizeDurationMs(long durationMs)
    {
        return durationMs switch
        {
            < 0 => 0,
            > int.MaxValue => int.MaxValue,
            _ => (int)durationMs
        };
    }

    // Kosuma bagli senaryo surumunu yukler; bagli senaryo yoksa icra edilecek belge de yoktur.
    /// <summary>Kosumun bagli senaryo surumunu getirir.</summary>
    private async Task<TestScenario> GetScenarioAsync(Guid? scenarioId, CancellationToken cancellationToken)
    {
        if (scenarioId is null)
        {
            throw new BusinessException(TestModuleRunErrorCodes.ScenarioDocumentRequired);
        }

        return await _testScenarioRepository.FindAsync(scenarioId.Value, cancellationToken: cancellationToken)
               ?? throw new EntityNotFoundException(typeof(TestScenario), scenarioId.Value);
    }

    // Yayinlanmamis veya derlenmemis bir senaryonun kosuma girmesini engeller.
    /// <summary>Senaryonun kosulabilir derlenmis belge tasidigini dogrular.</summary>
    private static string EnsureDocumentExists(TestScenario scenario)
    {
        return !string.IsNullOrWhiteSpace(scenario.CompiledDocument)
            ? scenario.CompiledDocument
            : throw new BusinessException(TestModuleRunErrorCodes.ScenarioDocumentRequired);
    }

    // Senaryonun yayin ani muhurlerini kosum aninda snapshot'lanan degerlerle karsilastirir.
    /// <summary>Kosum ani malzeme kaymasini degerlendirir.</summary>
    private TestRunMaterialDrift EvaluateDrift(TestScenario scenario, Entities.Runs.TestRun entity)
    {
        return _materialDriftManager.Evaluate(
            scenario,
            scenario.RulesFingerprint ?? string.Empty,
            entity.SpecFingerprint ?? string.Empty,
            entity.DbSchemaFingerprint ?? string.Empty,
            scenario.ProfileFingerprint ?? string.Empty);
    }

    // Ortam baglamasini secret degeri tasimayan runtime girdi sozlugune cevirir.
    /// <summary>Runner'a gecirilecek girdi sozlugunu ortam baglamasindan kurar.</summary>
    private static IReadOnlyDictionary<string, string> BuildInputs(
        TestRunEnvironmentBinding binding,
        string? traceId)
    {
        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [WorkflowRunnerConsts.Inputs.BaseUrl] = binding.BaseUrl,
            [WorkflowRunnerConsts.Inputs.EnvironmentKey] = binding.EnvironmentKey,
            [WorkflowRunnerConsts.Inputs.SpecSnapshotId] = binding.SpecSnapshotId.ToString("D", CultureInfo.InvariantCulture),
            [WorkflowRunnerConsts.Inputs.DbConnectionId] = binding.DbConnectionId.ToString("D", CultureInfo.InvariantCulture),
            [WorkflowRunnerConsts.Inputs.TraceId] = traceId ?? string.Empty
        };
    }
}
