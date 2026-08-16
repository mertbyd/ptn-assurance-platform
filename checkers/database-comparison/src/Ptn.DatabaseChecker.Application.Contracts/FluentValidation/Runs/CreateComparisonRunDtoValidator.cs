using FluentValidation;
using Ptn.DatabaseChecker.Dtos.Runs;
using Ptn.DatabaseChecker.ExceptionCodes;

namespace Ptn.DatabaseChecker.FluentValidation.Runs;

// islevi: ComparisonRun create isteginin girdi-format kurallarini tanimlar.
// sistemdeki gorevi: Run snapshot FK alanlarinin bos Guid olmamasini saglar; FK varlik kontrolu manager katmaninda batch/tekil yapilir.
public class CreateComparisonRunDtoValidator : AbstractValidator<CreateComparisonRunDto>
{
    public CreateComparisonRunDtoValidator()
    {
        // ComparisonDefinitionId opsiyoneldir; verilirse bos Guid olamaz.
        RuleFor(x => x.ComparisonDefinitionId)
            .Must(id => !id.HasValue || id.Value != System.Guid.Empty)
            .WithMessage(ComparisonRunExceptionCodes.Validation.ComparisonDefinitionIdInvalid);

        // SourceConnectionId bos Guid olamaz; baglanti varlik kontrolu manager katmaninda yapilir.
        RuleFor(x => x.SourceConnectionId)
            .NotEmpty().WithMessage(ComparisonRunExceptionCodes.Validation.SourceConnectionIdRequired);

        // TargetConnectionId bos Guid olamaz; baglanti varlik kontrolu manager katmaninda yapilir.
        RuleFor(x => x.TargetConnectionId)
            .NotEmpty().WithMessage(ComparisonRunExceptionCodes.Validation.TargetConnectionIdRequired);

        // ComparisonTypeId bos Guid olamaz; lookup varlik kontrolu manager katmaninda yapilir.
        RuleFor(x => x.ComparisonTypeId)
            .NotEmpty().WithMessage(ComparisonRunExceptionCodes.Validation.ComparisonTypeIdRequired);

        // StatusId bos Guid olamaz; lookup varlik kontrolu manager katmaninda yapilir.
        RuleFor(x => x.StatusId)
            .NotEmpty().WithMessage(ComparisonRunExceptionCodes.Validation.StatusIdRequired);
    }
}
