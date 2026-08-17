using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ptn.TestModule.Interface.Shared;
using Ptn.TestModule.Managers.Shared;
using Ptn.TestModule.Models.Shared;
using Volo.Abp.DependencyInjection;

namespace Ptn.TestModule.Services.Shared;

// islevi: Manager'in descriptor'ini dosya sistemi ve process framework cagri zinciriyle uygular.
// sistemdeki gorevi: Saf plan kararlarindan ayrilmis tek process ve filesystem I/O siniridir.
[ExposeServices(typeof(IProcessBoundaryPort))]
public sealed class ProcessBoundaryService : IProcessBoundaryPort, ITransientDependency
{
    private readonly ProcessPlanManager _manager;

    // Saf surec planini framework I/O sinirina baglar.
    public ProcessBoundaryService(ProcessPlanManager manager)
    {
        _manager = manager;
    }

    // Descriptor'daki workspace'i kurar, sureci kosar, artefaktlari okur ve workspace'i temizler.
    public async Task<ProcessExecutionOutcome> ExecuteAsync(
        ProcessExecutionPlan plan,
        CancellationToken cancellationToken = default)
    {
        var descriptor = _manager.CreateDescriptor(plan, Path.GetTempPath());
        foreach (var directory in descriptor.Workspace.Directories)
        {
            Directory.CreateDirectory(directory);
        }
        var stopwatch = Stopwatch.StartNew();
        var startInfo = new ProcessStartInfo
        {
            FileName = descriptor.Executable,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        foreach (var argument in descriptor.Arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }
        foreach (var variable in descriptor.EnvironmentVariables)
        {
            startInfo.Environment[variable.Key] = variable.Value;
        }

        ProcessExecutionOutcome outcome = default!;
        try
        {
            foreach (var input in descriptor.Workspace.InputFiles)
            {
                await File.WriteAllTextAsync(
                    input.RelativePath, input.Content, new UTF8Encoding(false), cancellationToken);
            }
            using var process = new Process { StartInfo = startInfo };
            try
            {
                _manager.EnsureStarted(process.Start(), descriptor);
            }
            catch (Win32Exception exception)
            {
                _manager.ThrowStartFailure(descriptor, exception);
            }
            var standardOutput = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var standardError = process.StandardError.ReadToEndAsync(cancellationToken);
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(descriptor.TimeoutMs);
            try
            {
                await process.WaitForExitAsync(timeout.Token);
            }
            catch (OperationCanceledException exception)
            {
                if (!process.HasExited)
                {
                    process.Kill(entireProcessTree: true);
                }
                _manager.ThrowCancellation(descriptor, exception, cancellationToken.IsCancellationRequested);
            }
            stopwatch.Stop();
            var outputs = new Dictionary<string, string?>(StringComparer.Ordinal);
            foreach (var output in descriptor.Workspace.OutputFiles)
            {
                outputs[output.Key] = File.Exists(output.Value)
                    ? await File.ReadAllTextAsync(output.Value, cancellationToken)
                    : null;
            }
            outcome = new ProcessExecutionOutcome
            {
                ExitCode = process.ExitCode,
                StandardOutput = await standardOutput,
                StandardError = await standardError,
                DurationMs = stopwatch.ElapsedMilliseconds,
                OutputFiles = outputs
            };
        }
        catch (Exception primary)
        {
            try
            {
                if (Directory.Exists(descriptor.Workspace.WorkspaceRoot))
                {
                    Directory.Delete(descriptor.Workspace.WorkspaceRoot, recursive: true);
                }
            }
            catch (IOException cleanupFailure)
            {
                _manager.RecordCleanupFailure(primary, cleanupFailure);
            }
            catch (UnauthorizedAccessException cleanupFailure)
            {
                _manager.RecordCleanupFailure(primary, cleanupFailure);
            }
            _manager.Rethrow(primary);
        }

        if (Directory.Exists(descriptor.Workspace.WorkspaceRoot))
        {
            Directory.Delete(descriptor.Workspace.WorkspaceRoot, recursive: true);
        }
        return outcome;
    }
}
