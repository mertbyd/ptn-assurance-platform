using FluentValidation;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Constants.Comparison.Projections;
using Ptn.DatabaseChecker.Dtos.Projections;
using Ptn.DatabaseChecker.ExceptionCodes;
using Ptn.DatabaseChecker.FluentValidation.Correlation;

namespace Ptn.DatabaseChecker.FluentValidation.Projections;

// islevi: Projection request'in kimlik, adres, anahtar, kolon, satir butcesi ve correlation seklini dogrular.
// sistemdeki gorevi: Canli katalog kararlarini Manager'a birakir; public girdinin sinirlarini I/O oncesi uygular.
public sealed class ProjectionRequestDtoValidator : AbstractValidator<ProjectionRequestDto>
{
    // islevi: Projection request-shape kurallarini kararli validation kodlariyla kaydeder.
    public ProjectionRequestDtoValidator()
    {
        RuleFor(item => item.ConnectionId)
            .NotEmpty().WithMessage(AssertionExceptionCodes.Validation.ConnectionIdRequired);
        RuleFor(item => item.SchemaName)
            .NotEmpty().WithMessage(AssertionExceptionCodes.Validation.SchemaRequired)
            .MaximumLength(SchemaObjectConsts.MaxSchemaNameLength)
            .WithMessage(AssertionExceptionCodes.Validation.SchemaMaxLength);
        RuleFor(item => item.TableName)
            .NotEmpty().WithMessage(AssertionExceptionCodes.Validation.TableRequired)
            .MaximumLength(SchemaObjectConsts.MaxObjectNameLength)
            .WithMessage(AssertionExceptionCodes.Validation.TableMaxLength);
        RuleFor(item => item.KeyValues)
            .NotEmpty().WithMessage(AssertionExceptionCodes.Validation.KeyRequired)
            .Must(keys => keys.Keys.All(key => !string.IsNullOrWhiteSpace(key)))
            .WithMessage(AssertionExceptionCodes.Validation.KeyColumnRequired);
        RuleFor(item => item.ProjectColumns)
            .NotEmpty().WithMessage(AssertionExceptionCodes.Validation.ProjectionColumnsRequired)
            .Must(columns => columns.Count <= ProjectionConsts.MaxProjectColumns)
            .WithMessage(AssertionExceptionCodes.Validation.ProjectionColumnsTooMany)
            .Must(columns => columns.All(column => !string.IsNullOrWhiteSpace(column)))
            .WithMessage(AssertionExceptionCodes.Validation.ProjectionColumnRequired);
        RuleFor(item => item.MaxRows!.Value)
            .InclusiveBetween(1, ProjectionConsts.MaxRowsCeiling)
            .WithMessage(AssertionExceptionCodes.Validation.ProjectionMaxRowsInvalid)
            .When(item => item.MaxRows.HasValue);
        RuleFor(item => item.Correlation!)
            .SetValidator(new CorrelationRefDtoValidator())
            .When(item => item.Correlation is not null);
    }
}
