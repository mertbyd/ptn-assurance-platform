using Ptn.TestModule.Constants.Runs.Lookups;
using Ptn.TestModule.Dtos.Runs;
using Ptn.TestModule.ExceptionCodes.Runs;
using Ptn.TestModule.FluentValidation.Runs;
using Shouldly;
using Xunit;

namespace Ptn.TestModule.FluentValidation.Runs;

// islevi: Kosum create, terminal ve nested bulgu DTO validator'larinin kodlu retlerini dogrular.
// sistemdeki gorevi: Public girdi sinirlarinin Manager ve DBML sozlesmesinden sapmasini engeller.
/// <summary>Test kosumu public girdi validator testleridir.</summary>
public class TestRunInputValidatorsTests
{
    /// <summary>Bos create girdisinin kararli zorunluluk kodlariyla reddedildigini dogrular.</summary>
    [Fact]
    public void Should_reject_empty_create_input_with_stable_codes()
    {
        var result = new CreateTestRunDtoValidator().Validate(new CreateTestRunDto());

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(error => error.ErrorCode == TestModuleRunErrorCodes.Validation.TestKeyRequired);
        result.Errors.ShouldContain(error => error.ErrorCode == TestModuleRunErrorCodes.Validation.EnvironmentKeyRequired);
        result.Errors.ShouldContain(error => error.ErrorCode == TestModuleRunErrorCodes.Validation.TriggerKindRequired);
    }

    /// <summary>Gecersiz fingerprint'in metin mesaji yerine kararli hata koduyla reddedildigini dogrular.</summary>
    [Fact]
    public void Should_reject_invalid_fingerprint_with_stable_code()
    {
        var input = CreateValidRunInput();
        input.SpecFingerprint = "not-a-sha256";

        var result = new CreateTestRunDtoValidator().Validate(input);

        result.Errors.ShouldContain(error => error.ErrorCode == TestModuleRunErrorCodes.Validation.FingerprintInvalid);
    }

    /// <summary>Negatif sure ve buyuk diagnosis raporunun ayri kararli kodlarla reddedildigini dogrular.</summary>
    [Fact]
    public void Should_reject_invalid_terminal_bounds_with_stable_codes()
    {
        var input = new WriteTestRunTerminalDto
        {
            OutcomeCode = "Failed",
            DurationMs = -1,
            DiagnosisReport = new string('x', 4097)
        };

        var result = new WriteTestRunTerminalDtoValidator().Validate(input);

        result.Errors.ShouldContain(error => error.ErrorCode == TestModuleRunErrorCodes.Validation.DurationInvalid);
        result.Errors.ShouldContain(error => error.ErrorCode == TestModuleRunErrorCodes.Validation.DiagnosisReportTooLarge);
    }

    /// <summary>Desteklenmeyen checker kaynaginin kararli validation koduyla reddedildigini dogrular.</summary>
    [Fact]
    public void Should_reject_unsupported_finding_source_with_stable_code()
    {
        var input = new TestResultFindingInputDto
        {
            SourceCheckerCode = "Unknown",
            ComparisonKindCode = "Equals",
            Location = "$.body.id",
            Message = "Mismatch"
        };

        var result = new TestResultFindingInputDtoValidator().Validate(input);

        result.Errors.ShouldContain(error => error.ErrorCode == TestModuleRunErrorCodes.Validation.SourceCheckerInvalid);
    }

    /// <summary>Sinirlara uyan create ve terminal girdilerinin validator zincirinden gectigini dogrular.</summary>
    [Fact]
    public void Should_accept_valid_run_inputs()
    {
        var terminal = new WriteTestRunTerminalDto
        {
            OutcomeCode = "Failed",
            DurationMs = 12,
            Findings =
            [
                new TestResultFindingInputDto
                {
                    Ordinal = 1,
                    SourceCheckerCode = TestSourceCheckerCodes.ApiContract,
                    ComparisonKindCode = "Equals",
                    Location = "$.body.id",
                    Message = "Mismatch"
                }
            ]
        };

        new CreateTestRunDtoValidator().Validate(CreateValidRunInput()).IsValid.ShouldBeTrue();
        new WriteTestRunTerminalDtoValidator().Validate(terminal).IsValid.ShouldBeTrue();
    }

    /// <summary>Validator testlerinde kullanilan en kucuk gecerli create girdisini kurar.</summary>
    private static CreateTestRunDto CreateValidRunInput()
    {
        return new CreateTestRunDto
        {
            TestKey = "checkout",
            EnvironmentKey = "staging",
            TriggerKindCode = "Manual",
            CanonicalInputs = "{}",
            SpecFingerprint = new string('a', 64),
            DbSchemaFingerprint = new string('b', 64)
        };
    }
}
