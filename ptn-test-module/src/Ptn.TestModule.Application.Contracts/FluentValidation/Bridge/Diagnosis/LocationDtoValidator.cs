using FluentValidation;
using Ptn.TestModule.Dtos.Bridge.Diagnosis;
using Ptn.TestModule.ExceptionCodes.Bridge;

namespace Ptn.TestModule.FluentValidation.Bridge.Diagnosis;

// islevi: Ortak diagnosis konumunun en az bir tipli adres parcasi tasidigini dogrular.
// sistemdeki gorevi: Tamamen bos konum navigation'ini kaynak checker'a gitmeden reddeder.
public sealed class LocationDtoValidator : AbstractValidator<LocationDto>
{
    public LocationDtoValidator()
    {
        RuleFor(input => input).Must(HasAddress)
            .WithMessage(TestModuleBridgeErrorCodes.Validation.LocationRequired);
    }

    private static bool HasAddress(LocationDto input) =>
        !string.IsNullOrWhiteSpace(input.ApiSchemaName) ||
        !string.IsNullOrWhiteSpace(input.DbSchemaName) ||
        !string.IsNullOrWhiteSpace(input.DbTableName) ||
        !string.IsNullOrWhiteSpace(input.ColumnName) ||
        !string.IsNullOrWhiteSpace(input.OperationId) ||
        !string.IsNullOrWhiteSpace(input.Method) ||
        !string.IsNullOrWhiteSpace(input.Path) ||
        !string.IsNullOrWhiteSpace(input.JsonPointer);
}
