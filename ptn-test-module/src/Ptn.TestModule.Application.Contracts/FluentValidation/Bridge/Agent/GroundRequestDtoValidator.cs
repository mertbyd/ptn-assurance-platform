using System.Linq;
using FluentValidation;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Dtos.Bridge;
using Ptn.TestModule.ExceptionCodes.Bridge;

namespace Ptn.TestModule.FluentValidation.Bridge.Agent;

// islevi: Ground girdisinin profil, referans ve response format seklini dogrular.
// sistemdeki gorevi: Eksik kimliklerin veya serbest format kodunun manager'a ulasmasini engeller.
public sealed class GroundRequestDtoValidator : AbstractValidator<GroundRequestDto>
{
    public GroundRequestDtoValidator()
    {
        RuleFor(input => input.ProfileKey).NotEmpty().WithMessage(TestModuleBridgeErrorCodes.Validation.ProfileKeyRequired);
        RuleFor(input => input.SpecSnapshotId).NotEmpty().WithMessage(TestModuleBridgeErrorCodes.Validation.SnapshotIdRequired);
        RuleFor(input => input.ConnectionId).NotEmpty().WithMessage(TestModuleBridgeErrorCodes.Validation.ConnectionIdRequired);
        RuleFor(input => input.OperationReferenceId).NotEmpty().WithMessage(TestModuleBridgeErrorCodes.Validation.OperationReferenceRequired);
        RuleFor(input => input.ResponseFormat).Must(PtnResponseFormatCodes.All.Contains)
            .WithMessage(TestModuleBridgeErrorCodes.Validation.ResponseFormatInvalid);
    }
}
