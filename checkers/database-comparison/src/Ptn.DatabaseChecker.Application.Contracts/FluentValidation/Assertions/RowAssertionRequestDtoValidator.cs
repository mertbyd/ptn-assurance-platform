using System.Linq;
using FluentValidation;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Dtos.Assertions;
using Ptn.DatabaseChecker.ExceptionCodes;
using Ptn.DatabaseChecker.FluentValidation.Correlation;

namespace Ptn.DatabaseChecker.FluentValidation.Assertions;

// islevi: Ortak assertion request DTO'sunun kimlik, adres, anahtar, nested matcher ve polling formatini dogrular.
// sistemdeki gorevi: Canli tablo/kolon/unique kontrollerini manager'a birakir; tum public assertion uclarinda ayni girdi seklini uygular.
public class RowAssertionRequestDtoValidator : AbstractValidator<RowAssertionRequestDto>
{
    // islevi: Ortak assertion request-shape kurallarini ve nested validator'lari kaydeder.
    public RowAssertionRequestDtoValidator()
    {
        RuleFor(item => item.ConnectionId)
            .NotEmpty().WithMessage(AssertionExceptionCodes.Validation.ConnectionIdRequired);
        RuleFor(item => item.SchemaName)
            .NotEmpty().WithMessage(AssertionExceptionCodes.Validation.SchemaRequired)
            .MaximumLength(SchemaObjectConsts.MaxSchemaNameLength).WithMessage(AssertionExceptionCodes.Validation.SchemaMaxLength);
        RuleFor(item => item.TableName)
            .NotEmpty().WithMessage(AssertionExceptionCodes.Validation.TableRequired)
            .MaximumLength(SchemaObjectConsts.MaxObjectNameLength).WithMessage(AssertionExceptionCodes.Validation.TableMaxLength);
        RuleFor(item => item.KeyValues)
            .NotEmpty().WithMessage(AssertionExceptionCodes.Validation.KeyRequired)
            .Must(keys => keys.Keys.All(key => !string.IsNullOrWhiteSpace(key)))
            .WithMessage(AssertionExceptionCodes.Validation.KeyColumnRequired);
        RuleFor(item => item.TimeoutMs)
            .GreaterThanOrEqualTo(0).WithMessage(AssertionExceptionCodes.Validation.TimeoutInvalid);
        RuleFor(item => item.PollIntervalMs)
            .GreaterThanOrEqualTo(0).WithMessage(AssertionExceptionCodes.Validation.PollIntervalInvalid);
        RuleFor(item => item.Cardinality).NotNull().SetValidator(new CardinalityExpectationDtoValidator());
        RuleForEach(item => item.Expectations).SetValidator(new ColumnExpectationDtoValidator());
        RuleFor(item => item.Correlation!)
            .SetValidator(new CorrelationRefDtoValidator())
            .When(item => item.Correlation is not null);
    }
}
