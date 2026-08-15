using Ptn.TestModule.Constants.Compilation;
using Ptn.TestModule.ExceptionCodes.Compilation;
using Ptn.TestModule.Managers.Compilation;
using Ptn.TestModule.Models.Shared;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace Ptn.TestModule.Compilation;

// islevi: Redocly lint cikis kodunun altyapi hatasi ile gecersiz belge arasindaki ayrimini dogrular.
// sistemdeki gorevi: Docker hatasinin sema hukmu sayilmasini ve gecersiz belgenin exception atmasini engeller.
public class ArazzoLintManagerTests
{
    // Imaj cekilemedi veya konteyner baslatilamadi durumu sema geçersizligi sayilmamalidir.
    [Theory]
    [InlineData(125)]
    [InlineData(126)]
    [InlineData(127)]
    public void Should_treat_docker_exit_codes_as_process_failure(int exitCode)
    {
        var exception = Should.Throw<BusinessException>(() =>
            new ArazzoLintManager().Interpret(Outcome(exitCode, "docker: command not found")));

        exception.Code.ShouldBe(TestModuleCompilationErrorCodes.LintProcessFailed);
    }

    // Gecersiz belge bir is sonucudur: exception atilmaz, kapi kodlu karar dondurur.
    [Fact]
    public void Should_report_invalid_document_without_throwing()
    {
        var result = new ArazzoLintManager().Interpret(Outcome(1, "struct-error: missing sourceDescriptions"));

        result.IsValid.ShouldBeFalse();
        result.Diagnostics.ShouldContain("missing sourceDescriptions");
    }

    // Temiz lint kosumu sema kapisini gecirmelidir.
    [Fact]
    public void Should_report_valid_document_on_zero_exit_code()
    {
        var result = new ArazzoLintManager().Interpret(Outcome(0, string.Empty));

        result.IsValid.ShouldBeTrue();
    }

    // Kalici tani alani butceyi asan lint ciktisinda kesilmelidir.
    [Fact]
    public void Should_bound_diagnostics_to_the_configured_budget()
    {
        var result = new ArazzoLintManager().Interpret(
            Outcome(1, new string('x', ArazzoCompilationConsts.MaxLintDiagnosticsLength + 512)));

        result.Diagnostics.Length.ShouldBe(ArazzoCompilationConsts.MaxLintDiagnosticsLength);
    }

    // Lint plani pinli imajla, salt-okunur mount ile ve kararli hata kodlariyla kurulmalidir.
    [Fact]
    public void Should_build_pinned_and_readonly_lint_plan()
    {
        var plan = new ArazzoLintManager().CreatePlan("arazzo: 1.0.1");

        plan.Executable.ShouldBe(ArazzoCompilationConsts.DockerExecutable);
        plan.Arguments.ShouldContain(ArazzoCompilationConsts.RedoclyCliImage);
        plan.Arguments.ShouldContain("lint");
        plan.Arguments.ShouldContain(argument => argument.Contains("readonly"));
        plan.TimeoutMs.ShouldBe(ArazzoCompilationConsts.LintTimeoutMs);
        plan.StartFailureErrorCode.ShouldBe(TestModuleCompilationErrorCodes.LintProcessFailed);
        plan.TimeoutErrorCode.ShouldBe(TestModuleCompilationErrorCodes.LintTimedOut);
        plan.InputFiles.ShouldHaveSingleItem().RelativePath
            .ShouldBe(ArazzoCompilationConsts.LintDocumentFileName);
    }

    // Surec sinirinin dondurdugu ham cikis nesnesini test icin kurar.
    private static ProcessExecutionOutcome Outcome(int exitCode, string standardError)
    {
        return new ProcessExecutionOutcome
        {
            ExitCode = exitCode,
            StandardOutput = string.Empty,
            StandardError = standardError
        };
    }
}
