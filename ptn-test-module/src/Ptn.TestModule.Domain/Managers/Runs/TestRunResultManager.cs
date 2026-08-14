using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

// islevi: Terminal hukum, attempt, diagnosis boyutu ve sirali bulgu invariantlarini uygular.
// sistemdeki gorevi: TestRun terminal gecisi ile TestRunResult aggregate kurulumunu tek atomik UoW icin hazirlar.
/// <summary>
/// Test kosumunun degismez terminal sonucu ve bulgularini olusturur.
/// </summary>
public class TestRunResultManager : FoundationManager<TestRunResult, Guid>
{
    /// <summary>Attempt ve bulgulu sonuc sorgularini saglayan repository'dir.</summary>
    private readonly ITestRunResultRepository _repository;

    /// <summary>Hukum kodlarini kalici lookup kimligine cozen repository'dir.</summary>
    private readonly ITestOutcomeStatusRepository _outcomeStatusRepository;

    /// <summary>Hata kategorisi kodlarini kalici lookup kimligine cozen repository'dir.</summary>
    private readonly ITestFailureCategoryRepository _failureCategoryRepository;

    /// <summary>TestRun aggregate'inin terminal durum mutasyonunu sahiplenen manager'dir.</summary>
    private readonly TestRunManager _testRunManager;

    /// <summary>Foundation tekillik kapisinin ayni attempt icin kullandigi hata kodudur.</summary>
    protected override string AlreadyExistsErrorCode => TestModuleRunErrorCodes.AttemptAlreadyWritten;

    // Sonuc, lookup ve kosum manager bagimliliklarini terminal yazim kuralina baglar.
    /// <summary>Terminal sonuc manager'ini repository, lookup ve run manager bagimliliklariyla kurar.</summary>
    public TestRunResultManager(
        ITestRunResultRepository repository,
        ITestOutcomeStatusRepository outcomeStatusRepository,
        ITestFailureCategoryRepository failureCategoryRepository,
        TestRunManager testRunManager)
        : base(repository)
    {
        _repository = repository;
        _outcomeStatusRepository = outcomeStatusRepository;
        _failureCategoryRepository = failureCategoryRepository;
        _testRunManager = testRunManager;
    }

    // Hukum, teshis ve bulgulari dogrular; run ile sonucu ayni UoW'da terminal yazima hazirlar.
    /// <summary>Yeni terminal sonucu uretip TestRun aggregate'ini Completed durumuna tasir.</summary>
    public async Task<TestRunResult> WriteAsync(
        TestRun testRun,
        TestRunTerminalModel model,
        int durationMs,
        DateTime completedAt,
        string? harBlobName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(testRun);
        ArgumentNullException.ThrowIfNull(model);
        if (durationMs < 0)
        {
            throw new BusinessException(TestModuleRunErrorCodes.DurationInvalid)
                .WithData(nameof(durationMs), durationMs);
        }
        var normalized = Normalize(model);
        EnsurePassedInvariant(normalized);
        EnsureDiagnosisReportSize(normalized.DiagnosisReport);
        var attempt = await GetNextAttemptAsync(testRun.Id, cancellationToken);
        var resultId = GuidGenerator.Create();
        var findings = CreateFindings(resultId, normalized.Findings);
        var result = new TestRunResult(
            resultId,
            testRun.Id,
            attempt,
            await GetOutcomeStatusIdAsync(normalized.OutcomeCode, cancellationToken),
            await GetFailureCategoryIdAsync(normalized.FailureCategoryCode, cancellationToken),
            durationMs,
            CurrentTenant.Id,
            normalized,
            findings);

        await _testRunManager.CompleteAsync(
            testRun,
            normalized.RunStatusCode,
            completedAt,
            harBlobName,
            cancellationToken);
        return result;
    }

    // Passed hukmunde sekiz sorun alaninin mutlak null olmasini zorunlu kilar.
    /// <summary>Passed terminal modelinin hicbir sorun konumu veya aciklamasi tasimadigini dogrular.</summary>
    public static void EnsurePassedInvariant(TestRunTerminalModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        var carriesFailureData = model.FailureCategoryCode is not null ||
                                 model.ErrorCode is not null ||
                                 model.Detail is not null ||
                                 model.FailedStepOrdinal is not null ||
                                 model.FailedStepName is not null ||
                                 model.FailedStepPath is not null ||
                                 model.TakenBranchPath is not null ||
                                 model.LastCompletedOrdinal is not null;
        if (model.OutcomeCode == TestOutcomeStatusCodes.Passed && carriesFailureData)
        {
            throw new BusinessException(TestModuleRunErrorCodes.PassedOutcomeCarriesFailureData);
        }
    }

    // Diagnosis JSON'ini UTF-8 bayt sayisiyla satir ici 4 KB butcesine baglar.
    /// <summary>Diagnosis raporunun UTF-8 boyutunun izin verilen siniri asmadigini dogrular.</summary>
    public static void EnsureDiagnosisReportSize(string? diagnosisReport)
    {
        if (diagnosisReport is not null &&
            Encoding.UTF8.GetByteCount(diagnosisReport) > TestRunResultConsts.MaxDiagnosisReportBytes)
        {
            throw new BusinessException(TestModuleRunErrorCodes.DiagnosisReportTooLarge);
        }
    }

    // En son attempt'i tek sorguda okuyup bir sonraki monoton degeri hesaplar.
    /// <summary>Verilen kosum icin bir sonraki bir tabanli attempt numarasini uretir.</summary>
    private async Task<int> GetNextAttemptAsync(Guid testRunId, CancellationToken cancellationToken)
    {
        var latest = await _repository.FindByAttemptAsync(testRunId, cancellationToken: cancellationToken);
        return latest?.Attempt + 1 ?? 1;
    }

    // Terminal metin alanlarini DBML uzunluklarina gore kanoniklestirir.
    /// <summary>Terminal modelinin metin ve bulgu alanlarini kalici yazima hazirlar.</summary>
    private static TestRunTerminalModel Normalize(TestRunTerminalModel model)
    {
        return new TestRunTerminalModel
        {
            OutcomeCode = NormalizeRequiredText(
                model.OutcomeCode,
                nameof(model.OutcomeCode),
                TestResultFindingConsts.MaxKindCodeLength),
            RunStatusCode = NormalizeRequiredText(
                model.RunStatusCode,
                nameof(model.RunStatusCode),
                TestResultFindingConsts.MaxKindCodeLength),
            FailureCategoryCode = NormalizeOptionalText(
                model.FailureCategoryCode,
                nameof(model.FailureCategoryCode),
                TestResultFindingConsts.MaxKindCodeLength),
            ErrorCode = NormalizeOptionalText(
                model.ErrorCode,
                nameof(model.ErrorCode),
                TestRunResultConsts.MaxErrorCodeLength),
            Detail = NormalizeOptionalText(model.Detail, nameof(model.Detail), TestRunResultConsts.MaxDetailLength),
            FailedStepOrdinal = model.FailedStepOrdinal,
            FailedStepName = NormalizeOptionalText(
                model.FailedStepName,
                nameof(model.FailedStepName),
                TestRunResultConsts.MaxStepNameLength),
            FailedStepPath = NormalizeOptionalText(
                model.FailedStepPath,
                nameof(model.FailedStepPath),
                TestRunResultConsts.MaxStepPathLength),
            TakenBranchPath = NormalizeOptionalText(
                model.TakenBranchPath,
                nameof(model.TakenBranchPath),
                TestRunResultConsts.MaxBranchPathLength),
            LastCompletedOrdinal = model.LastCompletedOrdinal,
            DiagnosisReport = model.DiagnosisReport,
            Findings = model.Findings
        };
    }

    // Bulgulari istenen ordinal'a gore siralar ve kesintisiz bir tabanli sirayla aggregate cocuguna cevirir.
    /// <summary>Terminal bulgularini normalize edip kararli sira numaralariyla entity koleksiyonu kurar.</summary>
    private IReadOnlyCollection<TestResultFinding> CreateFindings(
        Guid testRunResultId,
        IReadOnlyCollection<TestResultFindingModel> models)
    {
        var findings = new List<TestResultFinding>(models.Count);
        foreach (var model in models.OrderBy(item => item.Ordinal))
        {
            var normalized = Normalize(model);
            findings.Add(new TestResultFinding(
                GuidGenerator.Create(),
                testRunResultId,
                findings.Count + 1,
                CurrentTenant.Id,
                normalized));
        }

        return findings;
    }

    // Tek bulgunun acik uclu kod ve metin alanlarini guvenli DBML sinirlarina getirir.
    /// <summary>Bir terminal bulgusunu kaynak kodu ve uzunluk kurallariyla normalize eder.</summary>
    private static TestResultFindingModel Normalize(TestResultFindingModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        var sourceCheckerCode = NormalizeRequiredText(
            model.SourceCheckerCode,
            nameof(model.SourceCheckerCode),
            TestResultFindingConsts.MaxKindCodeLength);
        if (!TestSourceCheckerCodes.All.Contains(sourceCheckerCode))
        {
            throw new BusinessException(TestModuleRunErrorCodes.SourceCheckerNotSupported)
                .WithData(nameof(model.SourceCheckerCode), sourceCheckerCode);
        }

        return new TestResultFindingModel
        {
            Ordinal = model.Ordinal,
            SourceCheckerCode = sourceCheckerCode,
            ComparisonKindCode = NormalizeRequiredText(
                model.ComparisonKindCode,
                nameof(model.ComparisonKindCode),
                TestResultFindingConsts.MaxKindCodeLength),
            RuleRef = NormalizeOptionalText(model.RuleRef, nameof(model.RuleRef), TestResultFindingConsts.MaxRuleRefLength),
            Location = NormalizeRequiredText(model.Location, nameof(model.Location), TestResultFindingConsts.MaxLocationLength),
            TargetDisplayName = NormalizeOptionalText(
                model.TargetDisplayName,
                nameof(model.TargetDisplayName),
                TestResultFindingConsts.MaxTargetDisplayNameLength),
            Message = NormalizeRequiredText(model.Message, nameof(model.Message), TestResultFindingConsts.MaxMessageLength),
            ExpectedValue = NormalizeOptionalText(model.ExpectedValue, nameof(model.ExpectedValue), TestResultFindingConsts.MaxValueLength),
            ObservedValue = NormalizeOptionalText(model.ObservedValue, nameof(model.ObservedValue), TestResultFindingConsts.MaxValueLength),
            EvidenceSummary = NormalizeOptionalText(
                model.EvidenceSummary,
                nameof(model.EvidenceSummary),
                TestResultFindingConsts.MaxEvidenceSummaryLength),
            ObservedAtMs = model.ObservedAtMs,
            AttemptCount = model.AttemptCount
        };
    }

    // Hukum lookup kodunu aggregate'e yazilacak kimlige cozer.
    /// <summary>Verilen hukum kodunun lookup kimligini getirir.</summary>
    private async Task<Guid> GetOutcomeStatusIdAsync(
        string outcomeCode,
        CancellationToken cancellationToken)
    {
        var outcome = await _outcomeStatusRepository.FindAsync(
            new RepositoryQuery<TestOutcomeStatus>().Where(item => item.Code == outcomeCode),
            cancellationToken);
        return outcome?.Id ?? throw new EntityNotFoundException(typeof(TestOutcomeStatus));
    }

    // Opsiyonel hata kategorisi kodunu aggregate'e yazilacak kimlige cozer.
    /// <summary>Verilen opsiyonel hata kategorisi kodunun lookup kimligini getirir.</summary>
    private async Task<Guid?> GetFailureCategoryIdAsync(
        string? failureCategoryCode,
        CancellationToken cancellationToken)
    {
        if (failureCategoryCode is null)
        {
            return null;
        }

        var category = await _failureCategoryRepository.FindAsync(
            new RepositoryQuery<TestFailureCategory>().Where(item => item.Code == failureCategoryCode),
            cancellationToken);
        return category?.Id ?? throw new EntityNotFoundException(typeof(TestFailureCategory));
    }
}
