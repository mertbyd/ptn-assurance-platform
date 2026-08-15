using FluentValidation;
using Ptn.TestModule.Dtos.Bridge.Database;
using Ptn.TestModule.ExceptionCodes.Bridge;

namespace Ptn.TestModule.FluentValidation.Bridge.Database;

// islevi: Tek kolon beklentisinin adres ve matcher tasima seklini dogrular.
// sistemdeki gorevi: Gecersiz nested assertion girdisini checker cagrisindan once reddeder.
public sealed class ColumnExpectationDtoValidator : AbstractValidator<ColumnExpectationDto>
{
    public ColumnExpectationDtoValidator()
    {
        RuleFor(input => input.ColumnName).NotEmpty()
            .WithMessage(TestModuleBridgeErrorCodes.Validation.ColumnNameRequired);
        RuleFor(input => input.MatcherKindCode).NotEmpty()
            .WithMessage(TestModuleBridgeErrorCodes.Validation.MatcherKindRequired);
    }
}
