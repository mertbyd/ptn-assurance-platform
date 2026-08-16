using FluentValidation;
using Ptn.ApiContractChecker.Constants.Conformance;
using Ptn.ApiContractChecker.Dtos.Conformance;

namespace Ptn.ApiContractChecker.FluentValidation.Conformance;

// islevi: G2 girdisinin operasyon secimini ve sinirli JSON Pointer listesini dogrular.
public class AssertionDerivabilityDtoValidator : AbstractValidator<AssertionDerivabilityDto>
{
    public AssertionDerivabilityDtoValidator()
    {
        RuleFor(input => input.SnapshotId).NotEmpty();
        RuleFor(input => input.Method).NotEmpty().When(input => string.IsNullOrWhiteSpace(input.OperationId));
        RuleFor(input => input.Path).NotEmpty().When(input => string.IsNullOrWhiteSpace(input.OperationId));
        RuleFor(input => input.AssertionPaths).NotEmpty()
            .Must(paths => paths.Count <= ConformanceAuthoringConstants.MaxAssertionPaths);
        RuleForEach(input => input.AssertionPaths)
            .NotEmpty().MaximumLength(ConformanceAuthoringConstants.MaxAssertionPathLength)
            .Must(path => path.StartsWith('/'));
    }
}
