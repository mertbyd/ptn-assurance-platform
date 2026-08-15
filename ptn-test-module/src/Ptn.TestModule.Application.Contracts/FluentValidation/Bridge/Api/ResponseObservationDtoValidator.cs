using System.Linq;
using FluentValidation;
using Ptn.TestModule.Constants.Bridge.Vocabulary;
using Ptn.TestModule.Dtos.Bridge.Api;
using Ptn.TestModule.ExceptionCodes.Bridge;
using Ptn.TestModule.FluentValidation.Bridge.Correlation;

namespace Ptn.TestModule.FluentValidation.Bridge.Api;

// islevi: Response gozleminin snapshot, HTTP adresi ve status kodunu dogrular.
// sistemdeki gorevi: Sekilce gecersiz response assertion girdisini checker'dan once durdurur.
public sealed class ResponseObservationDtoValidator : AbstractValidator<ResponseObservationDto>
{
    public ResponseObservationDtoValidator()
    {
        RuleFor(input => input.SnapshotId).NotEmpty().WithMessage(TestModuleBridgeErrorCodes.Validation.SnapshotIdRequired);
        RuleFor(input => input.Method).NotEmpty().WithMessage(TestModuleBridgeErrorCodes.Validation.MethodRequired);
        RuleFor(input => input.Path).NotEmpty().WithMessage(TestModuleBridgeErrorCodes.Validation.PathRequired);
        RuleFor(input => input.StatusCode).InclusiveBetween(100, 599).WithMessage(TestModuleBridgeErrorCodes.Validation.StatusCodeInvalid);
        RuleFor(input => input.ProfileCode)
            .Must(profileCode => string.IsNullOrWhiteSpace(profileCode) ||
                                 PtnConformanceProfileCodes.All.Contains(profileCode))
            .WithMessage(TestModuleBridgeErrorCodes.Validation.ProfileCodeInvalid);
        RuleFor(input => input.Correlation!)
            .SetValidator(new CorrelationRefDtoValidator())
            .When(input => input.Correlation is not null);
    }
}
