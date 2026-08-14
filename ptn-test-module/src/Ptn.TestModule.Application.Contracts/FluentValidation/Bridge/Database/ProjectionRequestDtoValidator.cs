using FluentValidation;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Dtos.Bridge.Database;
using Ptn.TestModule.ExceptionCodes.Bridge;
using Ptn.TestModule.FluentValidation.Bridge.Correlation;

namespace Ptn.TestModule.FluentValidation.Bridge.Database;

// islevi: Database projeksiyon girdisinin adres ve satir butcesini dogrular.
// sistemdeki gorevi: Sinirsiz veya serbest projeksiyon istegini servis sinirinda engeller.
public sealed class ProjectionRequestDtoValidator : AbstractValidator<ProjectionRequestDto>
{
    public ProjectionRequestDtoValidator()
    {
        RuleFor(input => input.ConnectionId).NotEmpty().WithMessage(TestModuleBridgeErrorCodes.Validation.ConnectionIdRequired);
        RuleFor(input => input.DbSchemaName).NotEmpty().WithMessage(TestModuleBridgeErrorCodes.Validation.SchemaNameRequired);
        RuleFor(input => input.TableName).NotEmpty().WithMessage(TestModuleBridgeErrorCodes.Validation.TableNameRequired);
        RuleFor(input => input.MaxRows).InclusiveBetween(1, PtnBridgeConsts.MaxProjectionRows)
            .WithMessage(TestModuleBridgeErrorCodes.Validation.ProjectionRowLimitInvalid);
        RuleFor(input => input.Correlation!)
            .SetValidator(new CorrelationRefDtoValidator())
            .When(input => input.Correlation is not null);
    }
}
