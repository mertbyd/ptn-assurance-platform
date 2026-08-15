using FluentValidation;
using Ptn.TestModule.Dtos.Bridge.Database;
using Ptn.TestModule.ExceptionCodes.Bridge;

namespace Ptn.TestModule.FluentValidation.Bridge.Database;

// islevi: DB derivability adresinin katalog, kolon ve kapali kod alanlarini dogrular.
// sistemdeki gorevi: Sekilce eksik assertion'in canli katalog kapisina girmesini engeller.
public sealed class DatabaseDerivabilityAddressDtoValidator
    : AbstractValidator<DatabaseDerivabilityAddressDto>
{
    public DatabaseDerivabilityAddressDtoValidator()
    {
        RuleFor(input => input.SchemaName).NotEmpty()
            .WithMessage(TestModuleBridgeErrorCodes.Validation.SchemaNameRequired);
        RuleFor(input => input.TableName).NotEmpty()
            .WithMessage(TestModuleBridgeErrorCodes.Validation.TableNameRequired);
        RuleFor(input => input.KeyColumns).NotEmpty()
            .WithMessage(TestModuleBridgeErrorCodes.Validation.DerivabilityKeyColumnsRequired);
        RuleForEach(input => input.KeyColumns).NotEmpty()
            .WithMessage(TestModuleBridgeErrorCodes.Validation.ColumnNameRequired);
        RuleFor(input => input.ExpectedColumns).NotEmpty()
            .WithMessage(TestModuleBridgeErrorCodes.Validation.DerivabilityExpectedColumnsRequired);
        RuleForEach(input => input.ExpectedColumns).NotEmpty()
            .WithMessage(TestModuleBridgeErrorCodes.Validation.ColumnNameRequired);
        RuleFor(input => input.MatcherCode).NotEmpty()
            .WithMessage(TestModuleBridgeErrorCodes.Validation.MatcherKindRequired);
        RuleFor(input => input.CardinalityKindCode).NotEmpty()
            .WithMessage(TestModuleBridgeErrorCodes.Validation.CardinalityKindRequired);
    }
}
