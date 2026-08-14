using System.Linq;
using FluentValidation;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.Constants.Runs.Lookups;
using Ptn.TestModule.Dtos.Runs;
using Ptn.TestModule.ExceptionCodes.Runs;

namespace Ptn.TestModule.FluentValidation.Runs;

// islevi: Tek terminal bulgusunun kod, konum, metin ve sayisal alanlarini dogrular.
// sistemdeki gorevi: Kalici bulgu sinirlarini transport girisinde Manager kurallariyla ayni tutar.
/// <summary>Terminal sonuc bulgusu girdisini dogrular.</summary>
public sealed class TestResultFindingInputDtoValidator : AbstractValidator<TestResultFindingInputDto>
{
    /// <summary>Bulgu girdisinin tum public tasima kurallarini kurar.</summary>
    public TestResultFindingInputDtoValidator()
    {
        RuleFor(input => input.Ordinal)
            .GreaterThanOrEqualTo(0).WithErrorCode(TestModuleRunErrorCodes.Validation.StepOrdinalInvalid);
        RuleFor(input => input.SourceCheckerCode)
            .NotEmpty().WithErrorCode(TestModuleRunErrorCodes.Validation.SourceCheckerRequired)
            .Must(TestSourceCheckerCodes.All.Contains).WithErrorCode(TestModuleRunErrorCodes.Validation.SourceCheckerInvalid);
        RuleFor(input => input.ComparisonKindCode)
            .NotEmpty().WithErrorCode(TestModuleRunErrorCodes.Validation.ComparisonKindRequired)
            .MaximumLength(TestResultFindingConsts.MaxKindCodeLength).WithErrorCode(TestModuleRunErrorCodes.Validation.ComparisonKindTooLong);
        RuleFor(input => input.RuleRef)
            .MaximumLength(TestResultFindingConsts.MaxRuleRefLength).WithErrorCode(TestModuleRunErrorCodes.Validation.RuleRefTooLong);
        RuleFor(input => input.Location)
            .NotEmpty().WithErrorCode(TestModuleRunErrorCodes.Validation.LocationRequired)
            .MaximumLength(TestResultFindingConsts.MaxLocationLength).WithErrorCode(TestModuleRunErrorCodes.Validation.LocationTooLong);
        RuleFor(input => input.TargetDisplayName)
            .MaximumLength(TestResultFindingConsts.MaxTargetDisplayNameLength).WithErrorCode(TestModuleRunErrorCodes.Validation.TargetDisplayNameTooLong);
        RuleFor(input => input.Message)
            .NotEmpty().WithErrorCode(TestModuleRunErrorCodes.Validation.MessageRequired)
            .MaximumLength(TestResultFindingConsts.MaxMessageLength).WithErrorCode(TestModuleRunErrorCodes.Validation.MessageTooLong);
        RuleFor(input => input.ExpectedValue)
            .MaximumLength(TestResultFindingConsts.MaxValueLength).WithErrorCode(TestModuleRunErrorCodes.Validation.ValueTooLong);
        RuleFor(input => input.ObservedValue)
            .MaximumLength(TestResultFindingConsts.MaxValueLength).WithErrorCode(TestModuleRunErrorCodes.Validation.ValueTooLong);
        RuleFor(input => input.EvidenceSummary)
            .MaximumLength(TestResultFindingConsts.MaxEvidenceSummaryLength).WithErrorCode(TestModuleRunErrorCodes.Validation.EvidenceSummaryTooLong);
        RuleFor(input => input.ObservedAtMs)
            .Must(value => !value.HasValue || value.Value >= 0)
            .WithErrorCode(TestModuleRunErrorCodes.Validation.ObservedAtInvalid);
        RuleFor(input => input.AttemptCount)
            .Must(value => !value.HasValue || value.Value >= 0)
            .WithErrorCode(TestModuleRunErrorCodes.Validation.AttemptCountInvalid);
    }
}
