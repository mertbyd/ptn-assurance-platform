using System;
using System.Collections.Generic;
using System.Linq;
using Ptn.TestModule.Constants.Compilation;
using Ptn.TestModule.Constants.Shared;
using Ptn.TestModule.ExceptionCodes.Compilation;
using Ptn.TestModule.Models.Compilation;
using Ptn.TestModule.Models.Shared;
using Volo.Abp;

namespace Ptn.TestModule.Managers.Compilation;

// islevi: Pinli Redocly lint cagrisinin tam surec planini kurar ve ham surec cikisini sema hukmune cevirir.
// sistemdeki gorevi: Imaj, mount, butce ve cikis kodu siniflandirmasini tek domain sahibinde tutar; sinir yalniz kosar.
public class ArazzoLintManager : TestModuleDomainService
{
    // Docker altyapi hatasini Arazzo sema gecersizliginden ayiran kapali cikis kodu araligidir.
    private const int DockerFailureExitCodeMinimum = 125;
    private const int DockerFailureExitCodeMaximum = 127;

    // Belgeyi girdi dosyasi olarak tasiyan, salt-okunur mount ve pinli imajla kosan plani kurar.
    public ProcessExecutionPlan CreatePlan(string document)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(document);
        return new ProcessExecutionPlan
        {
            Executable = ArazzoCompilationConsts.DockerExecutable,
            Arguments = BuildArguments(),
            WorkspaceName = ArazzoCompilationConsts.LintWorkspaceName,
            InputFiles = [CreateDocumentFile(document)],
            TimeoutMs = ArazzoCompilationConsts.LintTimeoutMs,
            StartFailureErrorCode = TestModuleCompilationErrorCodes.LintProcessFailed,
            TimeoutErrorCode = TestModuleCompilationErrorCodes.LintTimedOut
        };
    }

    // Cikis kodunu once altyapi hatasi olarak eler, kalan durumu sema hukmune ve butceli taniya cevirir.
    public ArazzoLintResult Interpret(ProcessExecutionOutcome outcome)
    {
        ArgumentNullException.ThrowIfNull(outcome);
        var diagnostics = BoundDiagnostics(outcome);
        EnsureDockerInvocationSucceeded(outcome.ExitCode, diagnostics);
        return new ArazzoLintResult
        {
            IsValid = outcome.ExitCode == 0,
            Diagnostics = diagnostics
        };
    }

    // Lint edilecek belgeyi surec sinirinin yazacagi girdi dosyasina cevirir.
    private static ProcessInputFile CreateDocumentFile(string document)
    {
        return new ProcessInputFile
        {
            RelativePath = ArazzoCompilationConsts.LintDocumentFileName,
            Content = document
        };
    }

    // Belgeyi konteynere salt-okunur baglayip resmi lint komutunu kararli sirada kurar.
    private static IReadOnlyList<string> BuildArguments()
    {
        return
        [
            "run",
            "--rm",
            "--mount",
            $"type=bind,source={ProcessBoundaryConsts.WorkspaceToken},target=/spec,readonly",
            ArazzoCompilationConsts.RedoclyCliImage,
            "lint",
            $"/spec/{ArazzoCompilationConsts.LintDocumentFileName}",
            "--format=json",
            "--extends=spec"
        ];
    }

    // Imaj cekilemedi veya konteyner baslatilamadi durumunu gecersiz belge hukmunden ayirir.
    private static void EnsureDockerInvocationSucceeded(int exitCode, string diagnostics)
    {
        if (exitCode is >= DockerFailureExitCodeMinimum and <= DockerFailureExitCodeMaximum)
        {
            throw new BusinessException(TestModuleCompilationErrorCodes.LintProcessFailed)
                .WithData(nameof(diagnostics), diagnostics);
        }
    }

    // Iki akisi tek metinde birlestirip kalici tanisal veri butcesine indirger.
    private static string BoundDiagnostics(ProcessExecutionOutcome outcome)
    {
        var diagnostics = string.Join(
            Environment.NewLine,
            new[] { outcome.StandardOutput.Trim(), outcome.StandardError.Trim() }
                .Where(value => value.Length > 0));
        return diagnostics.Length <= ArazzoCompilationConsts.MaxLintDiagnosticsLength
            ? diagnostics
            : diagnostics[..ArazzoCompilationConsts.MaxLintDiagnosticsLength];
    }
}
