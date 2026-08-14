using System.Linq;
using FluentValidation;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Dtos.Bridge;
using Ptn.TestModule.ExceptionCodes.Bridge;

namespace Ptn.TestModule.FluentValidation.Bridge.Agent;

// islevi: Validate girdisinin profil, snapshot, operasyon, assertion ve response format seklini dogrular.
// sistemdeki gorevi: Bos assertion referansli bir istegin yayin kapisina Allow adayi olarak girmesini engeller.
public sealed class PtnValidateRequestDtoValidator : AbstractValidator<PtnValidateRequestDto>
{
    public PtnValidateRequestDtoValidator()
    {
        RuleFor(input => input.ProfileKey).NotEmpty().WithMessage(TestModuleBridgeErrorCodes.Validation.ProfileKeyRequired);
        RuleFor(input => input.ConnectionId).NotEmpty().WithMessage(TestModuleBridgeErrorCodes.Validation.ConnectionIdRequired);
        RuleFor(input => input.SpecSnapshotId).NotEmpty().WithMessage(TestModuleBridgeErrorCodes.Validation.SnapshotIdRequired);
        RuleFor(input => input.OperationReferenceId).NotEmpty().WithMessage(TestModuleBridgeErrorCodes.Validation.OperationReferenceRequired);
        RuleFor(input => input.AssertionReferenceIds).NotEmpty().WithMessage(TestModuleBridgeErrorCodes.Validation.AssertionReferenceRequired);
        RuleForEach(input => input.AssertionReferenceIds).NotEmpty().WithMessage(TestModuleBridgeErrorCodes.Validation.AssertionReferenceRequired);
        RuleFor(input => input.ResponseFormat).Must(PtnResponseFormatCodes.All.Contains)
            .WithMessage(TestModuleBridgeErrorCodes.Validation.ResponseFormatInvalid);
    }
}
