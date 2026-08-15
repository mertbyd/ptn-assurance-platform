using FluentValidation;
using Ptn.TestModule.Dtos.Bridge.Database;
using Ptn.TestModule.ExceptionCodes.Bridge;

namespace Ptn.TestModule.FluentValidation.Bridge.Database;

// islevi: Tablo sema sorgusunun baglanti, sema ve tablo adresini dogrular.
// sistemdeki gorevi: Gecersiz sema sorgusunu checker cagrisindan once durdurur.
public sealed class TableQueryDtoValidator : AbstractValidator<TableQueryDto>
{
    public TableQueryDtoValidator()
    {
        RuleFor(input => input.ConnectionId).NotEmpty().WithMessage(TestModuleBridgeErrorCodes.Validation.ConnectionIdRequired);
        RuleFor(input => input.DbSchemaName).NotEmpty().WithMessage(TestModuleBridgeErrorCodes.Validation.SchemaNameRequired);
        RuleFor(input => input.TableName).NotEmpty().WithMessage(TestModuleBridgeErrorCodes.Validation.TableNameRequired);
    }
}
