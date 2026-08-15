using FluentValidation;
using Ptn.TestModule.Dtos.Bridge.Database;
using Ptn.TestModule.ExceptionCodes.Bridge;

namespace Ptn.TestModule.FluentValidation.Bridge.Database;

// islevi: Database kardinalite beklentisinin kapali tur kodunu dogrular.
// sistemdeki gorevi: Bos nested kardinalite seklinin checker modeline sizmasini engeller.
public sealed class DatabaseCardinalityExpectationDtoValidator
    : AbstractValidator<DatabaseCardinalityExpectationDto>
{
    public DatabaseCardinalityExpectationDtoValidator()
    {
        RuleFor(input => input.KindCode).NotEmpty()
            .WithMessage(TestModuleBridgeErrorCodes.Validation.CardinalityKindRequired);
    }
}
