using FluentValidation;
using Ptn.TestModule.Dtos.Bridge.Api;
using Ptn.TestModule.ExceptionCodes.Bridge;

namespace Ptn.TestModule.FluentValidation.Bridge.Api;

// islevi: Response gozleminin snapshot, HTTP adresi ve status kodunu dogrular.
// sistemdeki gorevi: Sekilce gecersiz response assertion girdisini checker'dan once durdurur.
public sealed class ResponseObservationDtoValidator : AbstractValidator<ResponseObservationDto>
{
    public ResponseObservationDtoValidator()
    {
        RuleFor(input => input.SnapshotId).NotEmpty().WithErrorCode(TestModuleBridgeValidationErrorCodes.SnapshotIdRequired);
        RuleFor(input => input.Method).NotEmpty().WithErrorCode(TestModuleBridgeValidationErrorCodes.MethodRequired);
        RuleFor(input => input.Path).NotEmpty().WithErrorCode(TestModuleBridgeValidationErrorCodes.PathRequired);
        RuleFor(input => input.StatusCode).InclusiveBetween(100, 599).WithErrorCode(TestModuleBridgeValidationErrorCodes.StatusCodeInvalid);
    }
}
