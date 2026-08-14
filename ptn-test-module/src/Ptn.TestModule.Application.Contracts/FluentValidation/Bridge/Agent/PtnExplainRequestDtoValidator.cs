using System.Linq;
using FluentValidation;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Constants.Bridge.Vocabulary;
using Ptn.TestModule.Dtos.Bridge;
using Ptn.TestModule.ExceptionCodes.Bridge;

namespace Ptn.TestModule.FluentValidation.Bridge.Agent;

// islevi: Explain girdisinin profil, referans, outcome, status ve response format seklini dogrular.
// sistemdeki gorevi: Teshis manager'ina yalniz kapali sozluk ve gecerli HTTP status girdisi verir.
public sealed class PtnExplainRequestDtoValidator : AbstractValidator<PtnExplainRequestDto>
{
    public PtnExplainRequestDtoValidator()
    {
        RuleFor(input => input.ProfileKey).NotEmpty().WithMessage(TestModuleBridgeErrorCodes.Validation.ProfileKeyRequired);
        RuleFor(input => input.SpecSnapshotId).NotEmpty().WithMessage(TestModuleBridgeErrorCodes.Validation.SnapshotIdRequired);
        RuleFor(input => input.ConnectionId).NotEmpty().WithMessage(TestModuleBridgeErrorCodes.Validation.ConnectionIdRequired);
        RuleFor(input => input.OperationReferenceId).NotEmpty().WithMessage(TestModuleBridgeErrorCodes.Validation.OperationReferenceRequired);
        RuleFor(input => input.OutcomeCode).Must(PtnOutcomeCodes.All.Contains)
            .WithMessage(TestModuleBridgeErrorCodes.Validation.RequestRequired);
        RuleFor(input => input.StatusCode).InclusiveBetween(100, 599).When(input => input.StatusCode.HasValue)
            .WithMessage(TestModuleBridgeErrorCodes.Validation.StatusCodeInvalid);
        RuleFor(input => input.ResponseFormat).Must(PtnResponseFormatCodes.All.Contains)
            .WithMessage(TestModuleBridgeErrorCodes.Validation.ResponseFormatInvalid);
    }
}
