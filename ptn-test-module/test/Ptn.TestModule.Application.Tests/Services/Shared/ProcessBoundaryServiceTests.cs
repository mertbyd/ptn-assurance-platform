using System;
using System.IO;
using System.Threading.Tasks;
using Ptn.TestModule.Constants.Shared;
using Ptn.TestModule.ExceptionCodes.Compilation;
using Ptn.TestModule.Managers.Shared;
using Ptn.TestModule.Models.Shared;
using Ptn.TestModule.Services.Shared;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace Ptn.TestModule.Application.Tests.Services.Shared;

// islevi: Surec sinirinin timeout ve calisma klasoru temizligi arasindaki cift hata sozlesmesini dogrular.
// sistemdeki gorevi: Oldurulen process handle'i birakmadiginda temizlik hatasinin asil timeout hatasini maskelemesini engeller.
public class ProcessBoundaryServiceTests
{
    // Butceyi asan surec plandaki kararli timeout koduna cevrilmelidir.
    [Fact]
    public async Task Should_translate_a_timed_out_process_to_the_planned_error_code()
    {
        var plan = CreateSleepingPlan("timeout", lockWorkspace: false);

        var exception = await Should.ThrowAsync<BusinessException>(
            () => new ProcessBoundaryService(new ProcessPlanManager()).ExecuteAsync(plan));

        exception.Code.ShouldBe(TestModuleCompilationErrorCodes.LintTimedOut);
        DeleteWorkspaceRoot(plan.WorkspaceName);
    }

    // Timeout ile temizlik hatasi birlikte olustugunda cagiran asil timeout kodunu gormelidir.
    [Fact]
    public async Task Should_not_let_a_failed_cleanup_mask_the_timeout_error()
    {
        var plan = CreateSleepingPlan("cleanup", lockWorkspace: true);

        var exception = await Should.ThrowAsync<BusinessException>(
            () => new ProcessBoundaryService(new ProcessPlanManager()).ExecuteAsync(plan));

        exception.Code.ShouldBe(TestModuleCompilationErrorCodes.LintTimedOut);
        exception.Data.Contains(ProcessBoundaryConsts.CleanupFailureDataKey).ShouldBeTrue();
        DeleteWorkspaceRoot(plan.WorkspaceName);
    }

    // Butceyi asan, istege bagli olarak calisma klasorunu silinemez hale getiren plan kurar.
    private static ProcessExecutionPlan CreateSleepingPlan(string name, bool lockWorkspace)
    {
        return new ProcessExecutionPlan
        {
            Executable = OperatingSystem.IsWindows() ? "powershell.exe" : "/bin/sh",
            Arguments = OperatingSystem.IsWindows()
                ? ["-NoProfile", "-Command", WindowsScript(lockWorkspace)]
                : ["-c", ShellScript(lockWorkspace)],
            WorkspaceName = $"boundary-{name}-{Guid.NewGuid():N}",
            InputFiles = [new ProcessInputFile { RelativePath = LockFileName, Content = "locked" }],
            TimeoutMs = 6000,
            StartFailureErrorCode = TestModuleCompilationErrorCodes.LintProcessFailed,
            TimeoutErrorCode = TestModuleCompilationErrorCodes.LintTimedOut
        };
    }

    // Windows'ta girdi dosyasini salt-okunur yapar; Directory.Delete boylece UnauthorizedAccessException atar.
    private static string WindowsScript(bool lockWorkspace)
    {
        var prefix = lockWorkspace
            ? $"Set-ItemProperty -LiteralPath '{ProcessBoundaryConsts.WorkspaceToken}\\{LockFileName}' -Name IsReadOnly -Value $true; "
            : string.Empty;
        return prefix + "Start-Sleep -Seconds 120";
    }

    // Unix'te calisma klasorunun yazma iznini kaldirir; icerigin silinmesi boylece reddedilir.
    private static string ShellScript(bool lockWorkspace)
    {
        var prefix = lockWorkspace
            ? $"chmod 500 '{ProcessBoundaryConsts.WorkspaceToken}'; "
            : string.Empty;
        return prefix + "sleep 120";
    }

    // Testin biraktigi silinemez klasoru yazma izinlerini geri vererek temizler.
    private static void DeleteWorkspaceRoot(string workspaceName)
    {
        var root = Path.Combine(Path.GetTempPath(), ProcessBoundaryConsts.TempRootName, workspaceName);
        if (!Directory.Exists(root))
        {
            return;
        }

        foreach (var file in Directory.GetFiles(root, "*", SearchOption.AllDirectories))
        {
            File.SetAttributes(file, FileAttributes.Normal);
        }

        Directory.Delete(root, recursive: true);
    }

    // Salt-okunur yapilacak girdi dosyasinin adidir.
    private const string LockFileName = "lock.txt";
}
