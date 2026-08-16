using FluentValidation;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Constants.Comparison.Assertions;
using Ptn.DatabaseChecker.Dtos.Assertions;
using Ptn.DatabaseChecker.ExceptionCodes;

namespace Ptn.DatabaseChecker.FluentValidation.Assertions;

// islevi: Tek kolon expectation DTO'sunun ad, matcher ve operand formatini dogrular.
// sistemdeki gorevi: Tip/katalog is kurallarini manager'a birakir; public sinirin kapali matcher ve gerekli operand seklini korur.
public class ColumnExpectationDtoValidator : AbstractValidator<ColumnExpectationDto>
{
    // islevi: Matcher ailesine gore gerekli request-shape kurallarini kaydeder.
    public ColumnExpectationDtoValidator()
    {
        RuleFor(item => item.ColumnName)
            .NotEmpty().WithMessage(AssertionExceptionCodes.Validation.ExpectationColumnRequired)
            .MaximumLength(SchemaObjectConsts.MaxColumnNameLength);
        RuleFor(item => item.MatcherKindCode)
            .NotEmpty().WithMessage(AssertionExceptionCodes.Validation.MatcherRequired)
            .Must(MatcherKindCodes.IsDefined).WithMessage(AssertionExceptionCodes.Validation.MatcherInvalid);
        RuleFor(item => item.ExpectedValue)
            .NotNull()
            .When(RequiresSingleExpectedValue);
        RuleFor(item => item.ExpectedValues)
            .NotEmpty()
            .When(item => item.MatcherKindCode == MatcherKindCodes.OneOf);
        RuleFor(item => item.Tolerance)
            .NotNull()
            .GreaterThanOrEqualTo(0)
            .WithMessage(AssertionExceptionCodes.Validation.ToleranceInvalid)
            .When(item => item.MatcherKindCode == MatcherKindCodes.WithinTolerance);
    }

    // islevi: Null ve liste matcher'lari disinda tek beklenen operand gerekip gerekmedigini bildirir.
    private static bool RequiresSingleExpectedValue(ColumnExpectationDto item)
        => item.MatcherKindCode is not MatcherKindCodes.IsNull
            and not MatcherKindCodes.IsNotNull
            and not MatcherKindCodes.OneOf;
}
