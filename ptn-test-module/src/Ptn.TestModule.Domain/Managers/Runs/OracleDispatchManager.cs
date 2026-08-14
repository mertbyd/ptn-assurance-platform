using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Ptn.TestModule.Constants.Bridge.Vocabulary;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.Constants.Runs.Lookups;
using Ptn.TestModule.ExceptionCodes.Runs;
using Ptn.TestModule.Models.Bridge;
using Ptn.TestModule.Models.Runs;

namespace Ptn.TestModule.Managers.Runs;

// islevi: Her HAR entry'sini dogru hakeme yonlendirir, hukumleri bulgulara cevirir ve teshis raporunu butceye indirger.
// sistemdeki gorevi: Uc hakemin kayit sahipligini source_checker_code ile ayiran tek domain sahibidir (ADR-0015 §E).
/// <summary>
/// HAR adimlarini hakemlere dagitir ve donen hukumleri bulguya cevirir.
/// </summary>
public class OracleDispatchManager : TestModuleDomainService
{
    /// <summary>Checker'in karar veremedigini bildiren uygunluk kodlaridir.</summary>
    private static readonly IReadOnlySet<string> InconclusiveOutcomeCodes =
        new HashSet<string>(StringComparer.Ordinal)
        {
            PtnOutcomeCodes.Unavailable,
            PtnOutcomeCodes.SnapshotNotFound,
            PtnOutcomeCodes.OperationNotResolved,
            PtnOutcomeCodes.SchemaNotResolved
        };

    /// <summary>Hukmu olumsuz saymayan uygunluk kodlaridir.</summary>
    private static readonly IReadOnlySet<string> PassingOutcomeCodes =
        new HashSet<string>(StringComparer.Ordinal)
        {
            PtnOutcomeCodes.Passed,
            PtnOutcomeCodes.PolicySuppressed
        };

    // Entry'yi checker'in bekledigi gozleme cevirir ve korelasyonu echo edilebilir bicimde tasir.
    /// <summary>Bir HAR entry'sini API uygunluk gozlemine cevirir.</summary>
    public ResponseObservation CreateObservation(HarEntryModel entry, TestRunExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(context);

        return new ResponseObservation
        {
            SnapshotId = context.EnvironmentBinding.SpecSnapshotId,
            Method = entry.Method,
            Path = ReadPath(entry.Url),
            StatusCode = entry.StatusCode,
            ContentType = entry.ResponseContentType,
            Body = ReadBodyElement(entry.ResponseBody),
            Correlation = CreateCorrelation(entry, context.TraceId)
        };
    }

    // Uygunluk hukmunu adim hukmune cevirir; adim kimligi cozulemediyse hukum baglanamaz.
    /// <summary>API uygunluk sonucunu adim hukmune cevirir.</summary>
    public StepJudgement JudgeResponse(HarEntryModel entry, ConformanceResult result)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(result);
        var judgement = new StepJudgement
        {
            Entry = entry,
            SourceCheckerCode = TestSourceCheckerCodes.ApiContract,
            CheckerOutcomeCode = result.OutcomeCode,
            OutcomeCode = ResolveOutcomeCode(result.OutcomeCode),
            FailureCategoryCode = ResolveFailureCategory(result.OutcomeCode, TestFailureCategoryCodes.Contract),
            Findings = CreateConformanceFindings(entry, result)
        };

        return EnsureBoundToStep(judgement);
    }

    // Veritabani adimini HAR yanitindan okur; adimi asla yeniden cagirmaz (ADR-0015 §D).
    /// <summary>Derlenmis veritabani assertion adimini HAR yanitindan yargilar.</summary>
    public StepJudgement JudgeDatabaseAssertion(HarEntryModel entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        var outcomeCode = ReadAssertionOutcome(entry.ResponseBody);
        var judgement = new StepJudgement
        {
            Entry = entry,
            SourceCheckerCode = TestSourceCheckerCodes.DatabaseComparison,
            CheckerOutcomeCode = outcomeCode ?? string.Empty,
            OutcomeCode = ResolveAssertionOutcomeCode(entry, outcomeCode),
            FailureCategoryCode = ResolveAssertionFailureCategory(entry, outcomeCode),
            ErrorCode = outcomeCode is null ? TestModuleRunErrorCodes.AssertionResponseUnreadable : null,
            Findings = CreateAssertionFindings(entry, outcomeCode)
        };

        return EnsureBoundToStep(judgement);
    }

    // Runner'in kendi hukmunu hizli on kapi bulgusu olarak kaydeder.
    /// <summary>Runner cikis kodunu hizli on kapi adim hukmune cevirir.</summary>
    public StepJudgement JudgeRunner(WorkflowRunOutcome outcome)
    {
        ArgumentNullException.ThrowIfNull(outcome);
        var passed = outcome.ExitCode == 0;

        return new StepJudgement
        {
            SourceCheckerCode = TestSourceCheckerCodes.Runner,
            CheckerOutcomeCode = outcome.RunnerRef,
            OutcomeCode = passed ? TestOutcomeStatusCodes.Passed : TestOutcomeStatusCodes.Failed,
            FailureCategoryCode = passed ? null : TestFailureCategoryCodes.Business,
            ErrorCode = passed ? null : TestModuleRunErrorCodes.RunnerExitedNonZero,
            Findings = passed ? [] : [CreateRunnerFinding(outcome)]
        };
    }

    // Teshis edilecek ilk kirmizi adimi secer; rapor satirinda tek teshis tutulur.
    /// <summary>Teshis cagrisi yapilacak birincil basarisiz adim hukmunu getirir.</summary>
    public static StepJudgement? SelectDiagnosisTarget(IReadOnlyList<StepJudgement> judgements)
    {
        ArgumentNullException.ThrowIfNull(judgements);
        return judgements.FirstOrDefault(judgement =>
            judgement.OutcomeCode == TestOutcomeStatusCodes.Failed &&
            judgement.SourceCheckerCode != TestSourceCheckerCodes.Runner);
    }

    // Secilen kirmizi adimi checker'in bekledigi teshis sinyaline cevirir.
    /// <summary>Basarisiz adim hukmunu ortak teshis istegine cevirir.</summary>
    public DiagnosisRequest CreateDiagnosisRequest(StepJudgement judgement, TestRunExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(judgement);
        ArgumentNullException.ThrowIfNull(context);

        return new DiagnosisRequest
        {
            SpecSnapshotId = context.EnvironmentBinding.SpecSnapshotId,
            ConnectionId = context.EnvironmentBinding.DbConnectionId,
            Location = CreateLocation(judgement.Entry),
            StatusCode = judgement.Entry.StatusCode,
            ContentType = judgement.Entry.ResponseContentType,
            OutcomeCode = judgement.CheckerOutcomeCode,
            ObservedAtMs = judgement.Entry.StartedAtMs,
            Correlation = CreateCorrelation(judgement.Entry, context.TraceId)
        };
    }

    // Adim hukumlerini kararli sirali bulgu listesine ve butceli teshise indirger.
    /// <summary>Adim hukumlerini tek dagitim sonucunda birlestirir.</summary>
    public OracleDispatchResult Combine(IReadOnlyList<StepJudgement> judgements, DiagnosisReport? diagnosis)
    {
        ArgumentNullException.ThrowIfNull(judgements);

        return new OracleDispatchResult
        {
            Judgements = judgements,
            Findings = OrderFindings(judgements),
            DiagnosisReport = BoundDiagnosis(diagnosis)
        };
    }

    // Adim kimligi cozulemeyen bir hukum dogru adima baglanamaz; konumla eslemek yerine belirsiz kalir.
    /// <summary>Adim kimligi olmayan hukmu Inconclusive gerekceye indirger (ADR-0021).</summary>
    private static StepJudgement EnsureBoundToStep(StepJudgement judgement)
    {
        if (judgement.Entry.StepKey is not null)
        {
            return judgement;
        }

        judgement.OutcomeCode = TestOutcomeStatusCodes.Inconclusive;
        judgement.FailureCategoryCode = TestFailureCategoryCodes.Technical;
        judgement.ErrorCode = TestModuleRunErrorCodes.StepKeyMissing;
        return judgement;
    }

    // Checker'in ayrintili kodunu uc degerli adim hukmune indirger.
    /// <summary>Uygunluk kodunu adim hukum koduna cevirir.</summary>
    private static string ResolveOutcomeCode(string checkerOutcomeCode)
    {
        if (PassingOutcomeCodes.Contains(checkerOutcomeCode))
        {
            return TestOutcomeStatusCodes.Passed;
        }

        return InconclusiveOutcomeCodes.Contains(checkerOutcomeCode)
            ? TestOutcomeStatusCodes.Inconclusive
            : TestOutcomeStatusCodes.Failed;
    }

    // Kategori yalniz gercek bir hakem reddinde tasinir.
    /// <summary>Uygunluk koduna karsilik gelen hata kategorisini getirir.</summary>
    private static string? ResolveFailureCategory(string checkerOutcomeCode, string failedCategoryCode)
    {
        if (PassingOutcomeCodes.Contains(checkerOutcomeCode))
        {
            return null;
        }

        return InconclusiveOutcomeCodes.Contains(checkerOutcomeCode)
            ? TestFailureCategoryCodes.Technical
            : failedCategoryCode;
    }

    // Okunamayan yanit veya basarisiz checker cagrisi kalicilik hukmu vermez.
    /// <summary>Veritabani adiminin hukum kodunu belirler.</summary>
    private static string ResolveAssertionOutcomeCode(HarEntryModel entry, string? outcomeCode)
    {
        if (outcomeCode is null || entry.StatusCode >= 400)
        {
            return TestOutcomeStatusCodes.Inconclusive;
        }

        return ResolveOutcomeCode(outcomeCode);
    }

    // Kalicilik reddini checker cagrisi hatasindan ayirir.
    /// <summary>Veritabani adiminin hata kategorisini belirler.</summary>
    private static string? ResolveAssertionFailureCategory(HarEntryModel entry, string? outcomeCode)
    {
        if (outcomeCode is null || entry.StatusCode >= 400)
        {
            return TestFailureCategoryCodes.Technical;
        }

        return ResolveFailureCategory(outcomeCode, TestFailureCategoryCodes.Persistence);
    }

    // Her ihlali ayri bulguya cevirir; deger tasimadan kural ve konum kaniti birakir.
    /// <summary>Uygunluk ihlallerini kalici bulgulara cevirir.</summary>
    private static IReadOnlyList<TestResultFindingModel> CreateConformanceFindings(
        HarEntryModel entry,
        ConformanceResult result)
    {
        if (PassingOutcomeCodes.Contains(result.OutcomeCode))
        {
            return [];
        }

        if (result.Violations.Count == 0)
        {
            return [CreateFinding(entry, TestSourceCheckerCodes.ApiContract, result.OutcomeCode, null, null)];
        }

        return result.Violations
            .Select(violation => CreateFinding(
                entry,
                TestSourceCheckerCodes.ApiContract,
                result.OutcomeCode,
                violation.RuleCode,
                violation.JsonPointer))
            .ToList();
    }

    // Kalicilik reddini tek bulguya cevirir; gecen adim bulgu uretmez.
    /// <summary>Veritabani adiminin bulgularini olusturur.</summary>
    private static IReadOnlyList<TestResultFindingModel> CreateAssertionFindings(
        HarEntryModel entry,
        string? outcomeCode)
    {
        if (outcomeCode is null || PassingOutcomeCodes.Contains(outcomeCode))
        {
            return [];
        }

        return [CreateFinding(entry, TestSourceCheckerCodes.DatabaseComparison, outcomeCode, null, null)];
    }

    // Runner'in akis kontrolu reddini kaynak kodu Runner olan tek bulguya cevirir.
    /// <summary>Runner cikis kodunun bulgusunu olusturur.</summary>
    private static TestResultFindingModel CreateRunnerFinding(WorkflowRunOutcome outcome)
    {
        return new TestResultFindingModel
        {
            Ordinal = 0,
            SourceCheckerCode = TestSourceCheckerCodes.Runner,
            ComparisonKindCode = RespectCheckCodes.SuccessCriteriaCheck,
            Location = outcome.RunnerRef,
            Message = TestModuleRunErrorCodes.RunnerExitedNonZero,
            ObservedValue = outcome.ExitCode.ToString(),
            EvidenceSummary = outcome.JsonSummary.Length <= HarArtifactConsts.MaxInlineEvidenceBytes
                ? outcome.JsonSummary
                : outcome.JsonSummary[..HarArtifactConsts.MaxInlineEvidenceBytes]
        };
    }

    // Bulguyu adim konumu, kural referansi ve hakem koduyla kurar.
    /// <summary>Tek bir hakem bulgusunu kalici modele cevirir.</summary>
    private static TestResultFindingModel CreateFinding(
        HarEntryModel entry,
        string sourceCheckerCode,
        string comparisonKindCode,
        string? ruleRef,
        string? jsonPointer)
    {
        return new TestResultFindingModel
        {
            Ordinal = entry.Ordinal,
            SourceCheckerCode = sourceCheckerCode,
            ComparisonKindCode = comparisonKindCode,
            RuleRef = ruleRef,
            Location = CreateFindingLocation(entry, jsonPointer),
            TargetDisplayName = entry.StepKey,
            Message = comparisonKindCode,
            ObservedValue = entry.StatusCode.ToString(),
            ObservedAtMs = entry.StartedAtMs
        };
    }

    // Konumu adim kimligi, HTTP adresi ve opsiyonel pointer'dan kurar.
    /// <summary>Bulgunun makine-okur konumunu uretir.</summary>
    private static string CreateFindingLocation(HarEntryModel entry, string? jsonPointer)
    {
        var location = $"{entry.Method} {ReadPath(entry.Url)}";
        return string.IsNullOrWhiteSpace(jsonPointer) ? location : $"{location}#{jsonPointer}";
    }

    // Bulgulari adim sirasina gore kararli hale getirir; kesintisiz numaralandirmayi sonuc manager'i yapar.
    /// <summary>Tum adim bulgularini kararli sirada birlestirir.</summary>
    private static IReadOnlyList<TestResultFindingModel> OrderFindings(IReadOnlyList<StepJudgement> judgements)
    {
        return judgements
            .SelectMany(judgement => judgement.Findings)
            .OrderBy(finding => finding.Ordinal)
            .ToList();
    }

    // Raporu once tam serilestirir, butceyi asarsa hipotez ve kanit govdesini birakir.
    /// <summary>Teshis raporunu satir ici 4 KB butcesine indirger.</summary>
    private static string? BoundDiagnosis(DiagnosisReport? diagnosis)
    {
        if (diagnosis is null)
        {
            return null;
        }

        var serialized = JsonSerializer.Serialize(diagnosis);
        return Encoding.UTF8.GetByteCount(serialized) <= TestRunResultConsts.MaxDiagnosisReportBytes
            ? serialized
            : JsonSerializer.Serialize(CreateSummary(diagnosis));
    }

    // Butceyi asan raporu kaynak, baslik, konum ve tek hipoteze indirger.
    /// <summary>Butceyi asan teshis raporunun ozetini kurar.</summary>
    private static DiagnosisReport CreateSummary(DiagnosisReport diagnosis)
    {
        return new DiagnosisReport
        {
            SourceCheckerCode = diagnosis.SourceCheckerCode,
            Type = diagnosis.Type,
            Title = diagnosis.Title,
            Status = diagnosis.Status,
            Detail = diagnosis.Detail,
            Instance = diagnosis.Instance,
            Location = diagnosis.Location,
            Hypotheses = diagnosis.Hypotheses.Take(1).ToList(),
            Correlation = diagnosis.Correlation
        };
    }

    // Teshis konumunu adimin HTTP adresinden kurar.
    /// <summary>Basarisiz adimin teshis konumunu kurar.</summary>
    private static Location CreateLocation(HarEntryModel entry)
    {
        return new Location
        {
            Method = entry.Method,
            Path = ReadPath(entry.Url)
        };
    }

    // Korelasyonu trace ve cozulmus adim kimliginden kurar (ADR-0021).
    /// <summary>Checker cagrisinin korelasyon referansini kurar.</summary>
    private static CorrelationRef CreateCorrelation(HarEntryModel entry, string traceId)
    {
        return new CorrelationRef
        {
            TraceId = traceId,
            StepKey = entry.StepKey
        };
    }

    // Yanit govdesini checker'in bekledigi JSON elemanina cevirir; okunamazsa gonderilmez.
    /// <summary>Yanit govdesini tasinabilir JSON elemanina cevirir.</summary>
    private static JsonElement? ReadBodyElement(string? responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(responseBody);
            return document.RootElement.Clone();
        }
        catch (JsonException)
        {
            return null;
        }
    }

    // Veritabani checker yanitindaki hukum kodunu okur; okunamazsa hukum verilemez.
    /// <summary>Assertion yanitindaki hukum kodunu getirir.</summary>
    private static string? ReadAssertionOutcome(string? responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(responseBody);
            return document.RootElement.TryGetProperty(WorkflowRunnerConsts.OutcomeCodeField, out var value) &&
                   value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    // Mutlak adresi checker'in bekledigi yol bolumune indirger.
    /// <summary>Entry adresinin yol bolumunu getirir.</summary>
    private static string ReadPath(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var parsed) ? parsed.AbsolutePath : url;
    }
}
