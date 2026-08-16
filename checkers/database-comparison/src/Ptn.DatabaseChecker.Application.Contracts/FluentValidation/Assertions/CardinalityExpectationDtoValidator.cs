using FluentValidation;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Dtos.Assertions;
using Ptn.DatabaseChecker.ExceptionCodes;

namespace Ptn.DatabaseChecker.FluentValidation.Assertions;

// islevi: Cardinality expectation DTO'sunun kapali kod ve sayi formatini dogrular.
// sistemdeki gorevi: Count ve row uclarinin manager'a anlamli bir sayi iliskisi tasimasini saglar.
public class CardinalityExpectationDtoValidator : AbstractValidator<CardinalityExpectationDto>
{
    // islevi: Cardinality kodu ve matcher'a uygun expected-count araligini kaydeder.
    public CardinalityExpectationDtoValidator()
    {
        RuleFor(item => item.KindCode)
            .NotEmpty().WithMessage(AssertionExceptionCodes.Validation.CardinalityRequired)
            .Must(CardinalityKindCodes.IsDefined).WithMessage(AssertionExceptionCodes.Validation.CardinalityInvalid);
        RuleFor(item => item.ExpectedCount)
            .GreaterThanOrEqualTo(0)
            .WithMessage(AssertionExceptionCodes.Validation.ExpectedCountInvalid)
            .When(item => item.KindCode == CardinalityKindCodes.Exactly);
        RuleFor(item => item.ExpectedCount)
            .GreaterThan(0)
            .WithMessage(AssertionExceptionCodes.Validation.ExpectedCountInvalid)
            .When(item => item.KindCode == CardinalityKindCodes.AtLeast);
    }
}
