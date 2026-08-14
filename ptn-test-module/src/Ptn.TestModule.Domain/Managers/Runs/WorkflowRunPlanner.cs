using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.Constants.Runs.Lookups;
using Ptn.TestModule.ExceptionCodes.Runs;
using Ptn.TestModule.Models.Runs;
using Volo.Abp;
using Volo.Abp.Settings;

namespace Ptn.TestModule.Managers.Runs;

// islevi: Kosum belgesini kabul kapisindan gecirir, dort kontrolun severity'sini acikca set eder ve runner cagri planini kurar.
// sistemdeki gorevi: Arazzo surumu, XPath yasagi, girdi tasima yolu ve zaman asimi butcesinin tek domain sahibidir (ADR-0015 §A/§E/§G).
/// <summary>
/// Dis runner cagrisinin dogrulanmis istegini ve surec planini uretir.
/// </summary>
public class WorkflowRunPlanner : TestModuleDomainService
{
    /// <summary>Docker imajinin cekilemedigi cikis kodu araliginin alt sinirdir.</summary>
    private const int DockerFailureExitCodeMinimum = 125;

    /// <summary>Docker imajinin cekilemedigi cikis kodu araliginin ust sinirdir.</summary>
    private const int DockerFailureExitCodeMaximum = 127;

    /// <summary>Runner'in kontrol basarisizligini bildirdigi beklenen cikis kodudur.</summary>
    private const int CheckFailureExitCode = 1;

    /// <summary>Runner imaji ve zaman asimi butcesini cozen tenant-aware provider'dir.</summary>
    private readonly ISettingProvider _settingProvider;

    // Runner ayarlarini plan uretimine baglar.
    /// <summary>Planner'i aktif ABP setting provider ile kurar.</summary>
    public WorkflowRunPlanner(ISettingProvider settingProvider)
    {
        _settingProvider = settingProvider;
    }

    // Belgeyi kosulabilirlik kapisindan gecirir ve severity ile butceyi acikca doldurur.
    /// <summary>Dogrulanmis kosum istegini severity haritasi ve zaman asimi butcesiyle kurar.</summary>
    public async Task<WorkflowRunRequest> CreateRequestAsync(
        TestRunExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        cancellationToken.ThrowIfCancellationRequested();
        EnsureDocumentIsRunnable(context.DocumentFacts);

        return new WorkflowRunRequest
        {
            Document = context.CompiledDocument,
            Inputs = new Dictionary<string, string>(context.Inputs, StringComparer.Ordinal),
            SeverityMap = CreateSeverityMap(),
            ExecutionTimeoutSeconds = await ResolveBudgetAsync(
                TestModuleRunSettingNames.RunnerExecutionTimeoutSeconds,
                WorkflowRunnerConsts.DefaultExecutionTimeoutSeconds),
            MaxFetchTimeoutSeconds = await ResolveBudgetAsync(
                TestModuleRunSettingNames.RunnerMaxFetchTimeoutSeconds,
                WorkflowRunnerConsts.DefaultMaxFetchTimeoutSeconds),
            TraceId = context.TraceId
        };
    }

    // Imaji ayardan cozer, mount ve bayraklari kurar, girdileri argument yerine ortama tasir.
    /// <summary>Kosum istegini pinli imaj, mount ve bayraklardan olusan surec planina cevirir.</summary>
    public async Task<WorkflowRunPlan> CreatePlanAsync(
        WorkflowRunRequest request,
        string workingDirectory,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(workingDirectory);
        cancellationToken.ThrowIfCancellationRequested();
        var image = await ResolveImageAsync();

        return new WorkflowRunPlan
        {
            Executable = WorkflowRunnerConsts.DockerExecutable,
            Arguments = BuildArguments(request, workingDirectory, image),
            EnvironmentVariables = BuildEnvironmentVariables(request),
            DocumentFileName = WorkflowRunnerConsts.RunDocumentFileName,
            HarFileName = WorkflowRunnerConsts.HarOutputFileName,
            JsonFileName = WorkflowRunnerConsts.JsonOutputFileName,
            HardKillMs = ResolveHardKillMs(request.ExecutionTimeoutSeconds),
            RunnerRef = CreateRunnerRef(image)
        };
    }

    // Cikis kodunu once altyapi hatasi olarak eler, ardindan artefakt varligini ve boyutunu dogrular.
    /// <summary>Ham surec gozlemini dogrulanmis kosum ciktisina cevirir.</summary>
    public WorkflowRunOutcome Interpret(WorkflowRunProcessOutcome outcome, WorkflowRunPlan plan)
    {
        ArgumentNullException.ThrowIfNull(outcome);
        ArgumentNullException.ThrowIfNull(plan);
        EnsureExitCodeIsExpected(outcome.ExitCode);

        return new WorkflowRunOutcome
        {
            ExitCode = outcome.ExitCode,
            HarContent = EnsureHarIsUsable(outcome.HarContent),
            JsonSummary = BoundJsonSummary(outcome.JsonSummary),
            DurationMs = outcome.DurationMs,
            RunnerRef = plan.RunnerRef
        };
    }

    // Artefakti tenant, kosum ve trace kimliginden turetilen kararli blob adina baglar.
    /// <summary>HAR artefaktinin kalici depodaki blob adini uretir.</summary>
    public static string CreateHarBlobName(Guid? tenantId, Guid runId, string traceId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(traceId);
        return string.Format(
            CultureInfo.InvariantCulture,
            HarArtifactConsts.BlobNameFormat,
            tenantId?.ToString("N") ?? HarArtifactConsts.HostTenantSegment,
            runId.ToString("N"),
            traceId);
    }

    // Desteklenmeyen surumu ve XPath kriterini kosum baslamadan once reddeder.
    /// <summary>Belge olgularinin kosum kabul kapisini gectigini dogrular.</summary>
    private static void EnsureDocumentIsRunnable(WorkflowDocumentFacts facts)
    {
        if (facts.ArazzoVersion != WorkflowRunnerConsts.ArazzoTargetVersion)
        {
            throw new BusinessException(TestModuleRunErrorCodes.ArazzoVersionUnsupported)
                .WithData(nameof(facts.ArazzoVersion), facts.ArazzoVersion);
        }

        if (facts.HasXPathCriterion)
        {
            throw new BusinessException(TestModuleRunErrorCodes.XPathCriteriaRejected);
        }
    }

    // Dort kontrolun tamamini varsayilana birakmadan acikca set eder (AUDIT-0002 BULGU-08).
    /// <summary>Respect kontrollerinin kayit sahipligini koruyan severity haritasini kurar.</summary>
    private static IReadOnlyDictionary<string, string> CreateSeverityMap()
    {
        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [RespectCheckCodes.StatusCodeCheck] = RespectSeverityCodes.Error,
            [RespectCheckCodes.SuccessCriteriaCheck] = RespectSeverityCodes.Error,
            [RespectCheckCodes.SchemaCheck] = RespectSeverityCodes.Warn,
            [RespectCheckCodes.ContentTypeCheck] = RespectSeverityCodes.Warn
        };
    }

    // Belgeyi salt-okunur, artefakt klasorunu yazilabilir baglayip resmi respect komutunu kurar.
    /// <summary>Runner surecinin kararli argument listesini olusturur.</summary>
    private static IReadOnlyList<string> BuildArguments(
        WorkflowRunRequest request,
        string workingDirectory,
        string image)
    {
        return
        [
            "run",
            "--rm",
            "--env",
            WorkflowRunnerConsts.InputEnvironmentVariableName,
            "--mount",
            $"type=bind,source={workingDirectory}/{WorkflowRunnerConsts.DocumentDirectoryName},target={WorkflowRunnerConsts.DocumentMountTarget},readonly",
            "--mount",
            $"type=bind,source={workingDirectory}/{WorkflowRunnerConsts.OutputDirectoryName},target={WorkflowRunnerConsts.OutputMountTarget}",
            image,
            "respect",
            $"{WorkflowRunnerConsts.DocumentMountTarget}/{WorkflowRunnerConsts.RunDocumentFileName}",
            $"--har-output={WorkflowRunnerConsts.OutputMountTarget}/{WorkflowRunnerConsts.HarOutputFileName}",
            $"--json-output={WorkflowRunnerConsts.OutputMountTarget}/{WorkflowRunnerConsts.JsonOutputFileName}",
            $"--severity={JsonSerializer.Serialize(request.SeverityMap)}",
            $"--execution-timeout={request.ExecutionTimeoutSeconds.ToString(CultureInfo.InvariantCulture)}",
            $"--max-fetch-timeout={request.MaxFetchTimeoutSeconds.ToString(CultureInfo.InvariantCulture)}"
        ];
    }

    // Girdileri CLI bayragi yerine tek ortam degiskenine tasir (AUDIT-0002 BULGU-09).
    /// <summary>Runner girdilerini secret sizdirmayan ortam degiskeni sozlugune cevirir.</summary>
    private static IReadOnlyDictionary<string, string> BuildEnvironmentVariables(WorkflowRunRequest request)
    {
        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [WorkflowRunnerConsts.InputEnvironmentVariableName] = JsonSerializer.Serialize(request.Inputs)
        };
    }

    // Runner kendi butcesini asarsa surece taninacak toplam sert kill suresini hesaplar.
    /// <summary>Job seviyesindeki sert kill butcesini milisaniye olarak hesaplar.</summary>
    private static int ResolveHardKillMs(int executionTimeoutSeconds)
    {
        return checked((executionTimeoutSeconds * 1_000) + WorkflowRunnerConsts.HardKillGraceMs);
    }

    // Kosumun hangi runner surumuyle kostugunu test_runs satirina tasiyacak referansi uretir.
    /// <summary>Pinli imajdan kararli runner referansi uretir.</summary>
    private static string CreateRunnerRef(string image)
    {
        return string.Format(CultureInfo.InvariantCulture, WorkflowRunnerConsts.RunnerRefFormat, image);
    }

    // Kontrol basarisizligini normal sonuc kabul eder, altyapi ve bilinmeyen kodu ayirir.
    /// <summary>Runner cikis kodunun beklenen kumede oldugunu dogrular.</summary>
    private static void EnsureExitCodeIsExpected(int exitCode)
    {
        if (exitCode is >= DockerFailureExitCodeMinimum and <= DockerFailureExitCodeMaximum)
        {
            throw new BusinessException(TestModuleRunErrorCodes.RunnerImageUnavailable)
                .WithData(nameof(exitCode), exitCode);
        }

        if (exitCode is not 0 and not CheckFailureExitCode)
        {
            throw new BusinessException(TestModuleRunErrorCodes.RunnerExitedNonZero)
                .WithData(nameof(exitCode), exitCode);
        }
    }

    // Artefakt uretilmediyse veya depo butcesini astiysa yargi asamasina gecmez.
    /// <summary>HAR artefaktinin varligini ve boyut butcesini dogrular.</summary>
    private static string EnsureHarIsUsable(string? harContent)
    {
        if (string.IsNullOrWhiteSpace(harContent))
        {
            throw new BusinessException(TestModuleRunErrorCodes.HarNotProduced);
        }

        var byteCount = Encoding.UTF8.GetByteCount(harContent);
        if (byteCount > HarArtifactConsts.MaxHarBytes)
        {
            throw new BusinessException(TestModuleRunErrorCodes.HarTooLarge)
                .WithData(nameof(byteCount), byteCount);
        }

        return harContent;
    }

    // JSON ozetini kalici satir ici veri butcesine indirger.
    /// <summary>Runner JSON ozetini kararli uzunluk sinirina getirir.</summary>
    private static string BoundJsonSummary(string? jsonSummary)
    {
        if (string.IsNullOrWhiteSpace(jsonSummary))
        {
            return string.Empty;
        }

        return jsonSummary.Length <= WorkflowRunnerConsts.MaxJsonSummaryLength
            ? jsonSummary
            : jsonSummary[..WorkflowRunnerConsts.MaxJsonSummaryLength];
    }

    // Kosum imajini ayardan cozer, tanimsizsa pinli varsayilana duser.
    /// <summary>Kullanilacak runner imajini ayardan veya pinli varsayilandan getirir.</summary>
    private async Task<string> ResolveImageAsync()
    {
        var configured = await _settingProvider.GetOrNullAsync(TestModuleRunSettingNames.RunnerImage);
        return string.IsNullOrWhiteSpace(configured)
            ? WorkflowRunnerConsts.RedoclyCliImage
            : configured.Trim();
    }

    // Zaman asimi ayarini pozitif tam sayi olarak cozer, gecersiz degeri sessizce varsayilana indirmez.
    /// <summary>Verilen zaman asimi ayarini pozitif saniye degerine cozer.</summary>
    private async Task<int> ResolveBudgetAsync(string settingName, int defaultValue)
    {
        var configured = await _settingProvider.GetOrNullAsync(settingName);
        if (string.IsNullOrWhiteSpace(configured))
        {
            return defaultValue;
        }

        return int.TryParse(configured, NumberStyles.None, CultureInfo.InvariantCulture, out var parsed) && parsed > 0
            ? parsed
            : throw new BusinessException(TestModuleRunErrorCodes.RunnerTimedOut)
                .WithData(nameof(settingName), settingName);
    }
}
