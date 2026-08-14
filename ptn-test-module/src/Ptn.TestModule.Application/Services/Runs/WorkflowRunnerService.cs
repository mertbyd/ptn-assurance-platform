using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.ExceptionCodes.Runs;
using Ptn.TestModule.Interface.Runs;
using Ptn.TestModule.Managers.Runs;
using Ptn.TestModule.Models.Runs;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace Ptn.TestModule.Services.Runs;

// islevi: Arazzo belgesini olgulara cozer ve Planner'in kurdugu runner planini izole bir calisma klasorunde kosar.
// sistemdeki gorevi: Belge bicimi, dosya ve process ayrintisini tasiyan Application surec siniridir; bayrak, severity ve hukum kararlari Manager'dadir (ADR-0015 §A).
public sealed class WorkflowRunnerService : IWorkflowRunnerPort, ITransientDependency
{
    // Cagri planini ve ham surec gozleminin yorumunu sahiplenen Manager'dir.
    private readonly WorkflowRunPlanner _planner;

    // Surec sinirini plan ve yorum sahibi Manager'a baglar.
    public WorkflowRunnerService(WorkflowRunPlanner planner)
    {
        _planner = planner;
    }

    // Belgeyi tek geciste cozup Manager'in kabul kapisinin dayandigi uc olguyu dondurur.
    public WorkflowDocumentFacts ReadDocumentFacts(string document)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(document);
        var root = LoadDocumentRoot(document);

        return new WorkflowDocumentFacts
        {
            ArazzoVersion = ReadScalar(root, WorkflowRunnerConsts.ArazzoVersionField) ?? string.Empty,
            HasXPathCriterion = ContainsXPathCriterion(root),
            StepKeys = ReadStepKeys(root)
        };
    }

    // Belgeyi mount klasorune yazar, plani kosar ve yorumlanmis kosum ciktisini dondurur.
    public async Task<WorkflowRunOutcome> ExecuteAsync(
        WorkflowRunRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var workingDirectory = CreateWorkingDirectory();
        try
        {
            var plan = await _planner.CreatePlanAsync(request, workingDirectory, cancellationToken);
            await WriteDocumentAsync(workingDirectory, plan, request.Document, cancellationToken);
            var outcome = await RunAsync(plan, workingDirectory, cancellationToken);
            return _planner.Interpret(outcome, plan);
        }
        finally
        {
            DeleteWorkingDirectory(workingDirectory);
        }
    }

    // Belgeyi tek kok nesneli YAML dokumani olarak cozer; bozuk govdeyi kararli koda cevirir.
    private static YamlMappingNode LoadDocumentRoot(string document)
    {
        try
        {
            var stream = new YamlStream();
            stream.Load(new StringReader(document));
            if (stream.Documents.Count != 1 || stream.Documents[0].RootNode is not YamlMappingNode root)
            {
                throw new BusinessException(TestModuleRunErrorCodes.ArazzoVersionUnsupported);
            }

            return root;
        }
        catch (YamlException exception)
        {
            throw new BusinessException(
                TestModuleRunErrorCodes.ArazzoVersionUnsupported,
                innerException: exception);
        }
    }

    // Tum is akislarindaki adim kimliklerini kaynak sirasini koruyarak toplar.
    private static IReadOnlyList<string> ReadStepKeys(YamlMappingNode root)
    {
        return ReadSequence(root, WorkflowRunnerConsts.WorkflowsField)
            .OfType<YamlMappingNode>()
            .SelectMany(workflow => ReadSequence(workflow, WorkflowRunnerConsts.StepsField))
            .OfType<YamlMappingNode>()
            .Select(step => ReadScalar(step, WorkflowRunnerConsts.StepIdField))
            .Where(stepId => !string.IsNullOrWhiteSpace(stepId))
            .Select(stepId => stepId!)
            .ToList();
    }

    // Herhangi bir criterion nesnesindeki type=xpath degerini tum belge agacinda arar.
    private static bool ContainsXPathCriterion(YamlNode node)
    {
        if (node is YamlMappingNode mapping)
        {
            var type = ReadScalar(mapping, WorkflowRunnerConsts.CriterionTypeField);
            return string.Equals(type, WorkflowRunnerConsts.XPathCriterionType, StringComparison.OrdinalIgnoreCase) ||
                   mapping.Children.Any(item => ContainsXPathCriterion(item.Value));
        }

        return node is YamlSequenceNode sequence && sequence.Children.Any(ContainsXPathCriterion);
    }

    // Mapping alanini bos olmayan scalar metin olarak okur.
    private static string? ReadScalar(YamlMappingNode mapping, string field)
    {
        return mapping.Children.TryGetValue(new YamlScalarNode(field), out var node) &&
               node is YamlScalarNode scalar &&
               !string.IsNullOrWhiteSpace(scalar.Value)
            ? scalar.Value
            : null;
    }

    // Mapping alanini dizi olarak okur; alan yoksa bos dizi dondurur.
    private static IEnumerable<YamlNode> ReadSequence(YamlMappingNode mapping, string field)
    {
        return mapping.Children.TryGetValue(new YamlScalarNode(field), out var node) &&
               node is YamlSequenceNode sequence
            ? sequence.Children
            : [];
    }

    // Her kosum icin tahmin edilemez bir kok ile ayri belge ve artefakt klasorleri olusturur.
    private static string CreateWorkingDirectory()
    {
        var path = Path.Combine(
            Path.GetTempPath(),
            "ptn-test-module-arazzo-run",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(path, WorkflowRunnerConsts.DocumentDirectoryName));
        Directory.CreateDirectory(Path.Combine(path, WorkflowRunnerConsts.OutputDirectoryName));
        return path;
    }

    // Kosum belgesini plandaki adla BOM'suz UTF-8 olarak salt-okunur mount klasorune yazar.
    private static Task WriteDocumentAsync(
        string workingDirectory,
        WorkflowRunPlan plan,
        string document,
        CancellationToken cancellationToken)
    {
        return File.WriteAllTextAsync(
            Path.Combine(workingDirectory, WorkflowRunnerConsts.DocumentDirectoryName, plan.DocumentFileName),
            document,
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            cancellationToken);
    }

    // Plani shell birlestirmesi kullanmadan kosar, sureyi olcer ve artefaktlari ham cikis modeline toplar.
    private static async Task<WorkflowRunProcessOutcome> RunAsync(
        WorkflowRunPlan plan,
        string workingDirectory,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        using var process = new Process { StartInfo = CreateStartInfo(plan) };
        StartProcess(process);
        await WaitForExitAsync(process, plan.HardKillMs, cancellationToken);
        stopwatch.Stop();

        return new WorkflowRunProcessOutcome
        {
            ExitCode = process.ExitCode,
            HarContent = await ReadArtifactAsync(workingDirectory, plan.HarFileName, cancellationToken),
            JsonSummary = await ReadArtifactAsync(workingDirectory, plan.JsonFileName, cancellationToken),
            DurationMs = stopwatch.ElapsedMilliseconds
        };
    }

    // Plan argumanlarini ve girdi ortam degiskenlerini yeniden yorumlamadan ProcessStartInfo'ya tasir.
    private static ProcessStartInfo CreateStartInfo(WorkflowRunPlan plan)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = plan.Executable,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        foreach (var argument in plan.Arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        foreach (var variable in plan.EnvironmentVariables)
        {
            startInfo.Environment[variable.Key] = variable.Value;
        }

        return startInfo;
    }

    // Runner executable baslatma hatasini kararli kosum koduna cevirir.
    private static void StartProcess(Process process)
    {
        try
        {
            if (!process.Start())
            {
                throw new BusinessException(TestModuleRunErrorCodes.RunnerImageUnavailable);
            }
        }
        catch (Win32Exception exception)
        {
            throw new BusinessException(
                TestModuleRunErrorCodes.RunnerImageUnavailable,
                innerException: exception);
        }
    }

    // Sureci plandaki sert kill butcesiyle bekler; timeout ve dis iptali ayri davranislarla korur.
    private static async Task WaitForExitAsync(
        Process process,
        int hardKillMs,
        CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(hardKillMs);
        try
        {
            await process.WaitForExitAsync(timeout.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            KillProcessTree(process);
            throw new BusinessException(TestModuleRunErrorCodes.RunnerTimedOut);
        }
        catch (OperationCanceledException)
        {
            KillProcessTree(process);
            throw;
        }
    }

    // Iptal veya timeout sonrasinda runner process agacinin arka planda kalmasini engeller.
    private static void KillProcessTree(Process process)
    {
        if (!process.HasExited)
        {
            process.Kill(entireProcessTree: true);
        }
    }

    // Uretilmis artefakti okur; runner dosyayi hic yazmadiysa null dondurur.
    private static async Task<string?> ReadArtifactAsync(
        string workingDirectory,
        string fileName,
        CancellationToken cancellationToken)
    {
        var path = Path.Combine(workingDirectory, WorkflowRunnerConsts.OutputDirectoryName, fileName);
        return File.Exists(path)
            ? await File.ReadAllTextAsync(path, cancellationToken)
            : null;
    }

    // Her kosumun gecici belge ve artefakt klasorunu geri birakir.
    private static void DeleteWorkingDirectory(string workingDirectory)
    {
        if (Directory.Exists(workingDirectory))
        {
            Directory.Delete(workingDirectory, recursive: true);
        }
    }
}
