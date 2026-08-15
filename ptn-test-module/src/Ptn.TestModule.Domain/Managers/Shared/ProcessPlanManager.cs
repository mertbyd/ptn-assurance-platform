using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using Ptn.TestModule.Constants.Shared;
using Ptn.TestModule.Models.Shared;
using Volo.Abp;

namespace Ptn.TestModule.Managers.Shared;

// islevi: Surec planini dogrular, workspace ve baslatma descriptor'ini saf olarak kurar.
// sistemdeki gorevi: Yol, token, ortam, timeout ve hata kodu kararlarini process I/O sinirindan uzak tutar.
public class ProcessPlanManager : TestModuleDomainService
{
    // Ham plani tek seferde dogrulanmis workspace ve baslatma descriptor'ina cevirir.
    public ProcessStartDescriptor CreateDescriptor(ProcessExecutionPlan plan, string tempRoot)
    {
        EnsureValid(plan);
        var workspace = CreateWorkspaceLayout(plan, tempRoot);
        return new ProcessStartDescriptor
        {
            Executable = plan.Executable,
            Arguments = ResolveArguments(plan.Arguments, workspace.WorkspaceRoot),
            EnvironmentVariables = new Dictionary<string, string>(plan.EnvironmentVariables),
            Workspace = workspace,
            TimeoutMs = plan.TimeoutMs,
            StartFailureErrorCode = plan.StartFailureErrorCode,
            TimeoutErrorCode = plan.TimeoutErrorCode
        };
    }

    // Framework false sonucu donerse plandaki kararli baslatma kodunu firlatir.
    public void EnsureStarted(bool started, ProcessStartDescriptor descriptor)
    {
        if (!started)
        {
            throw new BusinessException(descriptor.StartFailureErrorCode);
        }
    }

    // Win32 baslatma kusurunu plandaki kararli koda cevirir.
    [DoesNotReturn]
    public void ThrowStartFailure(ProcessStartDescriptor descriptor, Win32Exception exception)
    {
        throw new BusinessException(descriptor.StartFailureErrorCode, innerException: exception);
    }

    // Dis iptali aynen korur; yalniz Manager butcesi dolduysa timeout koduna cevirir.
    [DoesNotReturn]
    public void ThrowCancellation(
        ProcessStartDescriptor descriptor,
        OperationCanceledException exception,
        bool externallyCancelled)
    {
        if (!externallyCancelled)
        {
            throw new BusinessException(descriptor.TimeoutErrorCode);
        }

        Rethrow(exception);
    }

    // Temizlik kusurunu asil hatanin kararli kanit anahtarina baglar.
    public void RecordCleanupFailure(Exception primary, Exception cleanupFailure)
    {
        primary.Data[ProcessBoundaryConsts.CleanupFailureDataKey] = cleanupFailure.ToString();
    }

    // Sinirda yakalanan asil hatayi stack bilgisini koruyarak yeniden firlatir.
    [DoesNotReturn]
    public void Rethrow(Exception exception)
    {
        ExceptionDispatchInfo.Capture(exception).Throw();
        throw new UnreachableException();
    }

    // Plan referansinin surec hazirligindan once var olmasini zorunlu kilar.
    private static void EnsureValid(ProcessExecutionPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
    }

    // Tahmin edilemez workspace kokunu ve tum tam dosya yollarini hesaplar.
    private static ProcessWorkspaceLayout CreateWorkspaceLayout(ProcessExecutionPlan plan, string tempRoot)
    {
        var root = Path.Combine(tempRoot, ProcessBoundaryConsts.TempRootName, plan.WorkspaceName, Guid.NewGuid().ToString("N"));
        var inputs = plan.InputFiles.Select(file => new ProcessInputFile
        {
            RelativePath = Path.Combine(root, file.RelativePath),
            Content = file.Content
        }).ToList();
        var outputs = plan.OutputFilePaths.Select(relativePath =>
            new KeyValuePair<string, string>(relativePath, Path.Combine(root, relativePath))).ToList();
        return new ProcessWorkspaceLayout
        {
            WorkspaceRoot = root,
            Directories = CreateDirectories(
                root,
                inputs.Select(file => file.RelativePath).Concat(outputs.Select(output => output.Value))),
            InputFiles = inputs,
            OutputFiles = outputs
        };
    }

    // Workspace koku ile her girdi ve artefaktin ust klasorunu olusturma listesine koyar.
    private static IReadOnlyList<string> CreateDirectories(string root, IEnumerable<string> filePaths)
    {
        return filePaths.Select(Path.GetDirectoryName)
            .Where(directory => !string.IsNullOrEmpty(directory))
            .Prepend(root)
            .Select(directory => directory!)
            .ToList();
    }

    // Workspace token'ini shell birlestirmesi yapmadan her argumentte cozer.
    private static IReadOnlyList<string> ResolveArguments(IReadOnlyList<string> arguments, string workspace)
    {
        return arguments.Select(argument => argument.Replace(
                ProcessBoundaryConsts.WorkspaceToken, workspace, StringComparison.Ordinal))
            .ToList();
    }
}
