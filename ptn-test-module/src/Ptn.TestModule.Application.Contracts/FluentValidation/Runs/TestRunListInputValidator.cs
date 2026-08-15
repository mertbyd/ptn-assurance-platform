using FluentValidation;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.Dtos.Runs;
using Ptn.TestModule.ExceptionCodes.Runs;

namespace Ptn.TestModule.FluentValidation.Runs;

// islevi: Kosum liste filtrelerinin bounded bicimini dogrular.
// sistemdeki gorevi: Tarih araligi, ortam ve sorting alanlarini repository'den once kapilar.
public sealed class TestRunListInputValidator : AbstractValidator<TestRunListInput>
{
    public TestRunListInputValidator()
    {
        RuleFor(input => input.EnvironmentKey)
            .MaximumLength(TestRunConsts.MaxEnvironmentKeyLength)
            .WithErrorCode(TestModuleRunErrorCodes.Validation.EnvironmentKeyTooLong);
        RuleFor(input => input)
            .Must(input => !input.CreatedFrom.HasValue || !input.CreatedTo.HasValue || input.CreatedFrom <= input.CreatedTo)
            .WithErrorCode(TestModuleRunErrorCodes.Validation.DateRangeInvalid);
        RuleFor(input => input.Sorting)
            .Must(value => value is null || value == TestRunQueryFields.CreationTime ||
                           value == TestRunQueryFields.StartedAt || value == TestRunQueryFields.CompletedAt ||
                           value == TestRunQueryFields.DurationMs || value == TestRunQueryFields.TestKey ||
                           value == TestRunQueryFields.EnvironmentKey)
            .WithErrorCode(TestModuleRunErrorCodes.Validation.SortingInvalid);
    }
}
