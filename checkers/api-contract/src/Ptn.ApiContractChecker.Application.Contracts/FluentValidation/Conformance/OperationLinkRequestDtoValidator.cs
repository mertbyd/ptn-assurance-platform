using FluentValidation;
using Ptn.ApiContractChecker.Constants.Conformance;
using Ptn.ApiContractChecker.Dtos.Conformance;
using Ptn.ApiContractChecker.ExceptionCodes.Conformance;

namespace Ptn.ApiContractChecker.FluentValidation.Conformance;

// islevi: Operation link isteginin snapshot, kaynak operationId ve aday butcesini dogrular.
// sistemdeki gorevi: Suggester'in serbest method/path veya sinirsiz aday girdisi almasini engeller.
public class OperationLinkRequestDtoValidator : AbstractValidator<OperationLinkRequestDto>
{
    public OperationLinkRequestDtoValidator()
    {
        RuleFor(input => input.SnapshotId).NotEmpty()
            .WithErrorCode(ConformanceExceptionCodes.SnapshotIdRequired);
        RuleFor(input => input.SourceOperationId).NotEmpty()
            .WithErrorCode(ConformanceExceptionCodes.SourceOperationIdRequired);
        RuleFor(input => input.MaxCandidates)
            .InclusiveBetween(1, SampleGenerationConsts.DefaultMaxCandidates)
            .WithErrorCode(ConformanceExceptionCodes.MaxCandidatesInvalid);
    }
}
