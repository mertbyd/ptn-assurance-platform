using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ptn.TestModule.Constants.Shared;
using Ptn.TestModule.Interface.Shared;
using Ptn.TestModule.Models.Shared;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace Ptn.TestModule.Services.Shared;

// islevi: Manager'in kurdugu surec planini izole bir gecici klasorde yazar, kosar, okur ve temizler.
// sistemdeki gorevi: Lint ve kosum sinirlarinin paylastigi tek process mekanigidir; hicbir bayrak, sure veya cikis kodu karari tasimaz.
public sealed class ProcessBoundaryService : IProcessBoundaryPort, ITransientDependency
{
    // Girdi dosyalarini yazar, sureci plandaki butceyle kosar ve artefaktlari geri okur.
    public async Task<ProcessExecutionOutcome> ExecuteAsync(
        ProcessExecutionPlan plan,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(plan);
        var workspace = CreateWorkspace(plan);
        try
        {
            await WriteInputFilesAsync(workspace, plan, cancellationToken);
            return await RunAsync(workspace, plan, cancellationToken);
        }
        finally
        {
            DeleteWorkspace(workspace);
        }
    }

    // Her cagri icin tahmin edilemez ve izole bir gecici klasor kokleri olusturur.
    private static string CreateWorkspace(ProcessExecutionPlan plan)
    {
        var workspace = Path.Combine(
            Path.GetTempPath(),
            ProcessBoundaryConsts.TempRootName,
            plan.WorkspaceName,
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workspace);
        foreach (var relativePath in plan.InputFiles.Select(file => file.RelativePath).Concat(plan.OutputFilePaths))
        {
            EnsureParentDirectory(workspace, relativePath);
        }

        return workspace;
    }

    // Artefakt ve belge yollarinin ust klasorlerini surec baslamadan once hazir eder.
    private static void EnsureParentDirectory(string workspace, string relativePath)
    {
        var parent = Path.GetDirectoryName(Path.Combine(workspace, relativePath));
        if (!string.IsNullOrEmpty(parent))
        {
            Directory.CreateDirectory(parent);
        }
    }

    // Plandaki girdi dosyalarini BOM'suz UTF-8 olarak calisma klasorune yazar.
    private static async Task WriteInputFilesAsync(
        string workspace,
        ProcessExecutionPlan plan,
        CancellationToken cancellationToken)
    {
        foreach (var file in plan.InputFiles)
        {
            await File.WriteAllTextAsync(
                Path.Combine(workspace, file.RelativePath),
                file.Content,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                cancellationToken);
        }
    }

    // Sureci shell birlestirmesi kullanmadan kosar, sureyi olcer ve akislarla artefaktlari toplar.
    private static async Task<ProcessExecutionOutcome> RunAsync(
        string workspace,
        ProcessExecutionPlan plan,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        using var process = new Process { StartInfo = CreateStartInfo(workspace, plan) };
        StartProcess(process, plan);
        var standardOutput = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var standardError = process.StandardError.ReadToEndAsync(cancellationToken);
        await WaitForExitAsync(process, plan, cancellationToken);
        stopwatch.Stop();

        return new ProcessExecutionOutcome
        {
            ExitCode = process.ExitCode,
            StandardOutput = await standardOutput,
            StandardError = await standardError,
            DurationMs = stopwatch.ElapsedMilliseconds,
            OutputFiles = await ReadOutputFilesAsync(workspace, plan, cancellationToken)
        };
    }

    // Plan argumanlarini yeniden yorumlamadan, yalniz calisma klasoru yer tutucusunu cozerek tasir.
    private static ProcessStartInfo CreateStartInfo(string workspace, ProcessExecutionPlan plan)
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
            startInfo.ArgumentList.Add(argument.Replace(
                ProcessBoundaryConsts.WorkspaceToken,
                workspace,
                StringComparison.Ordinal));
        }

        foreach (var variable in plan.EnvironmentVariables)
        {
            startInfo.Environment[variable.Key] = variable.Value;
        }

        return startInfo;
    }

    // Executable baslatma hatasini plandaki kararli koda cevirir.
    private static void StartProcess(Process process, ProcessExecutionPlan plan)
    {
        try
        {
            if (!process.Start())
            {
                throw new BusinessException(plan.StartFailureErrorCode);
            }
        }
        catch (Win32Exception exception)
        {
            throw new BusinessException(plan.StartFailureErrorCode, innerException: exception);
        }
    }

    // Sureci plandaki butceyle bekler; timeout ve dis iptali ayri davranislarla korur.
    private static async Task WaitForExitAsync(
        Process process,
        ProcessExecutionPlan plan,
        CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(plan.TimeoutMs);
        try
        {
            await process.WaitForExitAsync(timeout.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            KillProcessTree(process);
            throw new BusinessException(plan.TimeoutErrorCode);
        }
        catch (OperationCanceledException)
        {
            KillProcessTree(process);
            throw;
        }
    }

    // Iptal veya timeout sonrasinda process agacinin arka planda kalmasini engeller.
    private static void KillProcessTree(Process process)
    {
        if (!process.HasExited)
        {
            process.Kill(entireProcessTree: true);
        }
    }

    // Plandaki artefakt yollarini okur; surec dosyayi hic yazmadiysa deger null kalir.
    private static async Task<IReadOnlyDictionary<string, string?>> ReadOutputFilesAsync(
        string workspace,
        ProcessExecutionPlan plan,
        CancellationToken cancellationToken)
    {
        var outputs = new Dictionary<string, string?>(StringComparer.Ordinal);
        foreach (var relativePath in plan.OutputFilePaths)
        {
            var path = Path.Combine(workspace, relativePath);
            outputs[relativePath] = File.Exists(path)
                ? await File.ReadAllTextAsync(path, cancellationToken)
                : null;
        }

        return outputs;
    }

    // Her cagrinin gecici belge ve artefakt klasorunu geri birakir.
    private static void DeleteWorkspace(string workspace)
    {
        if (Directory.Exists(workspace))
        {
            Directory.Delete(workspace, recursive: true);
        }
    }
}
