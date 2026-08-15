using FluentValidation;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Dtos.Bridge.Api;
using Ptn.TestModule.ExceptionCodes.Bridge;

namespace Ptn.TestModule.FluentValidation.Bridge.Api;

// islevi: Operation link isteginin snapshot, kaynak operasyon ve aday butcesini dogrular.
// sistemdeki gorevi: Eksik kaynak veya sinirsiz aday girdisinin checker'a ulasmasini engeller.
public sealed class OperationLinkRequestDtoValidator : AbstractValidator<OperationLinkRequestDto>
{
    public OperationLinkRequestDtoValidator()
    {
        RuleFor(input => input.SnapshotId)
            .NotEmpty()
            .WithMessage(TestModuleBridgeErrorCodes.Validation.SnapshotIdRequired);
        RuleFor(input => input.SourceOperationId)
            .NotEmpty()
            .WithMessage(TestModuleBridgeErrorCodes.Validation.SourceOperationIdRequired);
        RuleFor(input => input.MaxCandidates)
            .InclusiveBetween(1, PtnBridgeConsts.MaxOperationLinkCandidates)
            .WithMessage(TestModuleBridgeErrorCodes.Validation.OperationLinkCandidateLimitInvalid);
    }
}
