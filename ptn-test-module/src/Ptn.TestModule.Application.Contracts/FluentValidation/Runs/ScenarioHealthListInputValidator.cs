using FluentValidation;
using Ptn.TestModule.Constants.Catalog;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.Dtos.Runs;
using Ptn.TestModule.ExceptionCodes.Runs;

namespace Ptn.TestModule.FluentValidation.Runs;

// islevi: Senaryo saglik liste filtrelerinin bounded bicimini dogrular.
// sistemdeki gorevi: Sayfa tavanini, oran araligini ve sorting token'ini repository'den once kapilar.
public sealed class ScenarioHealthListInputValidator : AbstractValidator<ScenarioHealthListInput>
{
    public ScenarioHealthListInputValidator()
    {
        RuleFor(input => input.MaxResultCount)
            .InclusiveBetween(1, ScenarioHealthConsts.MaxPageSize)
            .WithErrorCode(TestModuleRunErrorCodes.Validation.PageSizeInvalid);
        RuleFor(input => input.ScenarioKey)
            .MaximumLength(TestScenarioConsts.MaxScenarioKeyLength)
            .WithErrorCode(TestModuleRunErrorCodes.Validation.ScenarioKeyTooLong);
        RuleFor(input => input.MinFlakyRatio)
            .InclusiveBetween(0, 1)
            .When(input => input.MinFlakyRatio.HasValue)
            .WithErrorCode(TestModuleRunErrorCodes.Validation.RatioInvalid);
        RuleFor(input => input.MaxPassRatio)
            .InclusiveBetween(0, 1)
            .When(input => input.MaxPassRatio.HasValue)
            .WithErrorCode(TestModuleRunErrorCodes.Validation.RatioInvalid);
        RuleFor(input => input.Sorting)
            .Must(value => value is null || value == ScenarioHealthQueryFields.ScenarioKey ||
                           value == ScenarioHealthQueryFields.FlakyRatio ||
                           value == ScenarioHealthQueryFields.PassRatio ||
                           value == ScenarioHealthQueryFields.P95DurationMs ||
                           value == ScenarioHealthQueryFields.TotalRunCount)
            .WithErrorCode(TestModuleRunErrorCodes.Validation.SortingInvalid);
    }
}
