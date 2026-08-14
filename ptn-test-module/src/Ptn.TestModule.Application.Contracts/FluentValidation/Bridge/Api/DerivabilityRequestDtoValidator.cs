using FluentValidation;
using Ptn.TestModule.Dtos.Bridge.Api;
using Ptn.TestModule.ExceptionCodes.Bridge;

namespace Ptn.TestModule.FluentValidation.Bridge.Api;

// islevi: Assertion turetilebilirlik girdisinin operasyon ve pointer alanlarini dogrular.
// sistemdeki gorevi: Bos assertion sorgusunun checker'a ulasmasini engeller.
public sealed class DerivabilityRequestDtoValidator : AbstractValidator<DerivabilityRequestDto>
{
    public DerivabilityRequestDtoValidator()
    {
        RuleFor(input => input.SnapshotId).NotEmpty().WithErrorCode(TestModuleBridgeValidationErrorCodes.SnapshotIdRequired);
        RuleFor(input => input.Method).NotEmpty().WithErrorCode(TestModuleBridgeValidationErrorCodes.MethodRequired);
        RuleFor(input => input.Path).NotEmpty().WithErrorCode(TestModuleBridgeValidationErrorCodes.PathRequired);
        RuleFor(input => input.AssertionPaths).NotEmpty().WithErrorCode(TestModuleBridgeValidationErrorCodes.AssertionPathRequired);
        RuleForEach(input => input.AssertionPaths).NotEmpty().WithErrorCode(TestModuleBridgeValidationErrorCodes.AssertionPathRequired);
    }
}
