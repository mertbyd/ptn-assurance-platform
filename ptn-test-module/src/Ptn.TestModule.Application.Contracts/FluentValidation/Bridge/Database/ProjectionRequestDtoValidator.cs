using FluentValidation;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Dtos.Bridge.Database;
using Ptn.TestModule.ExceptionCodes.Bridge;

namespace Ptn.TestModule.FluentValidation.Bridge.Database;

// islevi: Database projeksiyon girdisinin adres ve satir butcesini dogrular.
// sistemdeki gorevi: Sinirsiz veya serbest projeksiyon istegini servis sinirinda engeller.
public sealed class ProjectionRequestDtoValidator : AbstractValidator<ProjectionRequestDto>
{
    public ProjectionRequestDtoValidator()
    {
        RuleFor(input => input.ConnectionId).NotEmpty().WithErrorCode(TestModuleBridgeValidationErrorCodes.ConnectionIdRequired);
        RuleFor(input => input.DbSchemaName).NotEmpty().WithErrorCode(TestModuleBridgeValidationErrorCodes.SchemaNameRequired);
        RuleFor(input => input.TableName).NotEmpty().WithErrorCode(TestModuleBridgeValidationErrorCodes.TableNameRequired);
        RuleFor(input => input.MaxRows).InclusiveBetween(1, PtnBridgeConsts.MaxProjectionRows)
            .WithErrorCode(TestModuleBridgeValidationErrorCodes.ProjectionRowLimitInvalid);
    }
}
