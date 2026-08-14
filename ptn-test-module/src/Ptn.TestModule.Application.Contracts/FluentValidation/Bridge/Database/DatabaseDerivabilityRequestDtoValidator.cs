using FluentValidation;
using Ptn.TestModule.Dtos.Bridge.Database;
using Ptn.TestModule.ExceptionCodes.Bridge;

namespace Ptn.TestModule.FluentValidation.Bridge.Database;

// islevi: DB derivability batch'inin baglanti ve assertion navigation seklini dogrular.
// sistemdeki gorevi: Bos bir assertion kumesinin yayin kapisinda basarili sayilmasini engeller.
public sealed class DatabaseDerivabilityRequestDtoValidator
    : AbstractValidator<DatabaseDerivabilityRequestDto>
{
    public DatabaseDerivabilityRequestDtoValidator()
    {
        RuleFor(input => input.ConnectionId).NotEmpty()
            .WithMessage(TestModuleBridgeErrorCodes.Validation.ConnectionIdRequired);
        RuleFor(input => input.Assertions).NotEmpty()
            .WithMessage(TestModuleBridgeErrorCodes.Validation.DerivabilityAssertionsRequired);
        RuleForEach(input => input.Assertions)
            .SetValidator(new DatabaseDerivabilityAddressDtoValidator());
    }
}
