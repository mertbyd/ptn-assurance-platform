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
        RuleFor(input => input.ConnectionId).NotEmpty().WithMessage(TestModuleBridgeErrorCodes.Validation.ConnectionIdRequired);
        RuleFor(input => input.SchemaName).NotEmpty().WithMessage(TestModuleBridgeErrorCodes.Validation.SchemaNameRequired);
        RuleFor(input => input.TableName).NotEmpty().WithMessage(TestModuleBridgeErrorCodes.Validation.TableNameRequired);
        RuleFor(input => input.TimeoutMs).GreaterThan(0).WithMessage(TestModuleBridgeErrorCodes.Validation.TimeoutInvalid);
        RuleFor(input => input.PollIntervalMs).GreaterThan(0).WithMessage(TestModuleBridgeErrorCodes.Validation.PollIntervalInvalid);
        RuleForEach(input => input.Expectations).SetValidator(new ColumnExpectationDtoValidator());
        RuleFor(input => input.Cardinality).NotNull()
            .WithMessage(TestModuleBridgeErrorCodes.Validation.RequestRequired)
            .SetValidator(new DatabaseCardinalityExpectationDtoValidator());
    }
}
