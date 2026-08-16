using FluentValidation;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Constants.Comparison.Assertions;
using Ptn.DatabaseChecker.Dtos.Assertions;
using Ptn.DatabaseChecker.ExceptionCodes;

namespace Ptn.DatabaseChecker.FluentValidation.Assertions;

// islevi: Derivability request'in baglanti, assertion adresi, kolon, matcher ve cardinality seklini dogrular.
// sistemdeki gorevi: Canli katalog/unique/tip kararlarindan once public input'un typed ve sinirli olmasini saglar.
public sealed class DerivabilityRequestDtoValidator : AbstractValidator<DerivabilityRequestDto>
{
    // islevi: Top-level ve oge-bazli derivability request-shape kurallarini kaydeder.
    public DerivabilityRequestDtoValidator()
    {
        RuleFor(item => item.ConnectionId)
            .NotEmpty().WithMessage(AssertionExceptionCodes.Validation.ConnectionIdRequired);
        RuleFor(item => item.Assertions)
            .NotEmpty().WithMessage(AssertionExceptionCodes.Validation.DerivabilityAssertionsRequired);
        RuleForEach(item => item.Assertions).ChildRules(address =>
        {
            address.RuleFor(item => item.SchemaName)
                .NotEmpty().WithMessage(AssertionExceptionCodes.Validation.SchemaRequired)
                .MaximumLength(SchemaObjectConsts.MaxSchemaNameLength)
                .WithMessage(AssertionExceptionCodes.Validation.SchemaMaxLength);
            address.RuleFor(item => item.TableName)
                .NotEmpty().WithMessage(AssertionExceptionCodes.Validation.TableRequired)
                .MaximumLength(SchemaObjectConsts.MaxObjectNameLength)
                .WithMessage(AssertionExceptionCodes.Validation.TableMaxLength);
            address.RuleFor(item => item.KeyColumns)
                .NotEmpty().WithMessage(AssertionExceptionCodes.Validation.DerivabilityKeyColumnsRequired)
                .Must(columns => columns.All(column => !string.IsNullOrWhiteSpace(column)))
                .WithMessage(AssertionExceptionCodes.Validation.KeyColumnRequired);
            address.RuleFor(item => item.ExpectedColumns)
                .NotEmpty().WithMessage(AssertionExceptionCodes.Validation.DerivabilityExpectedColumnsRequired)
                .Must(columns => columns.All(column => !string.IsNullOrWhiteSpace(column)))
                .WithMessage(AssertionExceptionCodes.Validation.ExpectationColumnRequired);
            address.RuleFor(item => item.MatcherCode)
                .NotEmpty().WithMessage(AssertionExceptionCodes.Validation.MatcherRequired)
                .Must(MatcherKindCodes.IsDefined)
                .WithMessage(AssertionExceptionCodes.Validation.MatcherInvalid);
            address.RuleFor(item => item.CardinalityKindCode)
                .NotEmpty().WithMessage(AssertionExceptionCodes.Validation.CardinalityRequired)
                .Must(CardinalityKindCodes.IsDefined)
                .WithMessage(AssertionExceptionCodes.Validation.CardinalityInvalid);
        });
    }
}
