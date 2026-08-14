using FluentValidation;
using Ptn.TestModule.Dtos.Bridge.Database;
using Ptn.TestModule.ExceptionCodes.Bridge;

namespace Ptn.TestModule.FluentValidation.Bridge.Database;

// islevi: Database assertion girdisinin adres ve polling butcesini dogrular.
// sistemdeki gorevi: Gecersiz tasima girdisini database checker cagrisindan once durdurur.
public sealed class DatabaseAssertionRequestDtoValidator : AbstractValidator<DatabaseAssertionRequestDto>
{
    public DatabaseAssertionRequestDtoValidator()
    {
        RuleFor(input => input.ConnectionId).NotEmpty().WithErrorCode(TestModuleBridgeValidationErrorCodes.ConnectionIdRequired);
        RuleFor(input => input.SchemaName).NotEmpty().WithErrorCode(TestModuleBridgeValidationErrorCodes.SchemaNameRequired);
        RuleFor(input => input.TableName).NotEmpty().WithErrorCode(TestModuleBridgeValidationErrorCodes.TableNameRequired);
        RuleFor(input => input.TimeoutMs).GreaterThan(0).WithErrorCode(TestModuleBridgeValidationErrorCodes.TimeoutInvalid);
        RuleFor(input => input.PollIntervalMs).GreaterThan(0).WithErrorCode(TestModuleBridgeValidationErrorCodes.PollIntervalInvalid);
        RuleForEach(input => input.Expectations).SetValidator(new ColumnExpectationDtoValidator());
        RuleFor(input => input.Cardinality).NotNull()
            .WithErrorCode(TestModuleBridgeValidationErrorCodes.RequestRequired)
            .SetValidator(new DatabaseCardinalityExpectationDtoValidator());
    }
}
