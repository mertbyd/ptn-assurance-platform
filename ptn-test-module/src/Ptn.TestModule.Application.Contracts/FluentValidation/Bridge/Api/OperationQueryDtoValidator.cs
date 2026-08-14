using FluentValidation;
using Ptn.TestModule.Dtos.Bridge.Api;
using Ptn.TestModule.ExceptionCodes.Bridge;

namespace Ptn.TestModule.FluentValidation.Bridge.Api;

// islevi: Operasyon sorgusunun zorunlu snapshot ve HTTP adres alanlarini dogrular.
// sistemdeki gorevi: Gecersiz tasima girdisini checker cagrisindan once durdurur.
public sealed class OperationQueryDtoValidator : AbstractValidator<OperationQueryDto>
{
    public OperationQueryDtoValidator()
    {
        RuleFor(input => input.SnapshotId).NotEmpty().WithErrorCode(TestModuleBridgeValidationErrorCodes.SnapshotIdRequired);
        RuleFor(input => input.Method).NotEmpty().WithErrorCode(TestModuleBridgeValidationErrorCodes.MethodRequired);
        RuleFor(input => input.Path).NotEmpty().WithErrorCode(TestModuleBridgeValidationErrorCodes.PathRequired);
    }
}
