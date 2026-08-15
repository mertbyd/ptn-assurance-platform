using System.Text;
using FluentValidation;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.Dtos.Runs;
using Ptn.TestModule.ExceptionCodes.Runs;

namespace Ptn.TestModule.FluentValidation.Runs;

// islevi: Terminal sonuc DTO'sunun hukum, sorun, sure, artefakt ve bulgu alanlarini dogrular.
// sistemdeki gorevi: Public girdinin kalici sinirlara uymasini ve nested bulgularin ayri validator'dan gecmesini saglar.
/// <summary>Test kosumu terminal yazma girdisini dogrular.</summary>
public sealed class WriteTestRunTerminalDtoValidator : AbstractValidator<WriteTestRunTerminalDto>
{
    /// <summary>Terminal yazim icin tum public tasima kurallarini kurar.</summary>
    public WriteTestRunTerminalDtoValidator()
    {
        RuleFor(input => input.OutcomeCode)
            .NotEmpty().WithErrorCode(TestModuleRunErrorCodes.Validation.OutcomeRequired)
            .MaximumLength(TestResultFindingConsts.MaxKindCodeLength).WithErrorCode(TestModuleRunErrorCodes.Validation.OutcomeTooLong);
        RuleFor(input => input.FailureCategoryCode)
            .MaximumLength(TestResultFindingConsts.MaxKindCodeLength).WithErrorCode(TestModuleRunErrorCodes.Validation.FailureCategoryTooLong);
        RuleFor(input => input.ErrorCode)
            .MaximumLength(TestRunResultConsts.MaxErrorCodeLength).WithErrorCode(TestModuleRunErrorCodes.Validation.ErrorCodeTooLong);
        RuleFor(input => input.Detail)
            .MaximumLength(TestRunResultConsts.MaxDetailLength).WithErrorCode(TestModuleRunErrorCodes.Validation.DetailTooLong);
        RuleFor(input => input.FailedStepOrdinal)
            .Must(value => !value.HasValue || value.Value > 0)
            .WithErrorCode(TestModuleRunErrorCodes.Validation.StepOrdinalInvalid);
        RuleFor(input => input.LastCompletedOrdinal)
            .Must(value => !value.HasValue || value.Value > 0)
            .WithErrorCode(TestModuleRunErrorCodes.Validation.StepOrdinalInvalid);
        RuleFor(input => input.FailedStepName)
            .MaximumLength(TestRunResultConsts.MaxStepNameLength).WithErrorCode(TestModuleRunErrorCodes.Validation.StepNameTooLong);
        RuleFor(input => input.FailedStepPath)
            .MaximumLength(TestRunResultConsts.MaxStepPathLength).WithErrorCode(TestModuleRunErrorCodes.Validation.StepPathTooLong);
        RuleFor(input => input.TakenBranchPath)
            .MaximumLength(TestRunResultConsts.MaxBranchPathLength).WithErrorCode(TestModuleRunErrorCodes.Validation.BranchPathTooLong);
        RuleFor(input => input.DurationMs)
            .GreaterThanOrEqualTo(0).WithErrorCode(TestModuleRunErrorCodes.Validation.DurationInvalid);
        RuleFor(input => input.HarBlobName)
            .MaximumLength(TestRunConsts.MaxHarBlobNameLength).WithErrorCode(TestModuleRunErrorCodes.Validation.HarBlobNameTooLong);
        RuleFor(input => input.DiagnosisReport)
            .Must(value => value is null || Encoding.UTF8.GetByteCount(value) <= TestRunResultConsts.MaxDiagnosisReportBytes)
            .WithErrorCode(TestModuleRunErrorCodes.Validation.DiagnosisReportTooLarge);
        RuleFor(input => input.Findings)
            .NotNull().WithErrorCode(TestModuleRunErrorCodes.Validation.FindingsRequired);
        RuleForEach(input => input.Findings)
            .SetValidator(new TestResultFindingInputDtoValidator());
    }
}
