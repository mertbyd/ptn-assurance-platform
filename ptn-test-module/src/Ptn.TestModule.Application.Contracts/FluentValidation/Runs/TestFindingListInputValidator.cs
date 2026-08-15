using FluentValidation;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.Dtos.Runs;
using Ptn.TestModule.ExceptionCodes.Runs;

namespace Ptn.TestModule.FluentValidation.Runs;

// islevi: Bulgu liste filtrelerinin uzunluk, tarih ve fingerprint butcesini dogrular.
// sistemdeki gorevi: Sinirsiz fingerprint ve serbest sorting sorgularini HTTP sinirinda reddeder.
public sealed class TestFindingListInputValidator : AbstractValidator<TestFindingListInput>
{
    public TestFindingListInputValidator()
    {
        RuleFor(input => input.Fingerprints)
            .NotNull().WithErrorCode(TestModuleRunErrorCodes.Validation.FingerprintsRequired)
            .Must(values => values.Count <= TestRunQueryFields.MaxFingerprintCount)
            .WithErrorCode(TestModuleRunErrorCodes.Validation.FingerprintLimitExceeded);
        RuleForEach(input => input.Fingerprints)
            .Matches(TestRunConsts.FingerprintPattern)
            .WithErrorCode(TestModuleRunErrorCodes.Validation.FingerprintInvalid);
        RuleFor(input => input.RuleRef)
            .MaximumLength(TestResultFindingConsts.MaxRuleRefLength)
            .WithErrorCode(TestModuleRunErrorCodes.Validation.RuleRefTooLong);
        RuleFor(input => input.SeverityCode)
            .Must(value => value is null || value == SarifReportConsts.Level.Error)
            .WithErrorCode(TestModuleRunErrorCodes.Validation.SeverityInvalid);
        RuleFor(input => input)
            .Must(input => !input.CreatedFrom.HasValue || !input.CreatedTo.HasValue || input.CreatedFrom <= input.CreatedTo)
            .WithErrorCode(TestModuleRunErrorCodes.Validation.DateRangeInvalid);
        RuleFor(input => input.Sorting)
            .Must(value => value is null || value == TestRunQueryFields.CreationTime || value == TestRunQueryFields.Attempt)
            .WithErrorCode(TestModuleRunErrorCodes.Validation.SortingInvalid);
    }
}
