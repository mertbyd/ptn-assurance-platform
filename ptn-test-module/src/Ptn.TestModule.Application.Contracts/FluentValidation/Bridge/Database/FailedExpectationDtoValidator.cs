using FluentValidation;
using Ptn.TestModule.Dtos.Bridge.Database;
using Ptn.TestModule.ExceptionCodes.Bridge;

namespace Ptn.TestModule.FluentValidation.Bridge.Database;

// islevi: Diagnosis girdisindeki basarisiz kolon beklentisinin adres ve matcher seklini dogrular.
// sistemdeki gorevi: Eksik nested assertion sinyalini checker cagrisindan once reddeder.
public sealed class FailedExpectationDtoValidator : AbstractValidator<FailedExpectationDto>
{
    public FailedExpectationDtoValidator()
    {
        RuleFor(input => input.ColumnName).NotEmpty()
            .WithErrorCode(TestModuleBridgeValidationErrorCodes.ColumnNameRequired);
        RuleFor(input => input.MatcherKindCode).NotEmpty()
            .WithErrorCode(TestModuleBridgeValidationErrorCodes.MatcherKindRequired);
    }
}
