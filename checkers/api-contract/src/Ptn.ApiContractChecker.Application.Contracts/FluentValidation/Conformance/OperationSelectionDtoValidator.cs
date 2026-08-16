using FluentValidation;
using Ptn.ApiContractChecker.Dtos.Conformance;
using Ptn.ApiContractChecker.ExceptionCodes.Conformance;
using Ptn.ApiContractChecker.Constants.Snapshots.Lookups;

namespace Ptn.ApiContractChecker.FluentValidation.Conformance;

// islevi: Request ornegi ve baglama onerisi operasyon seciminin public input seklini dogrular.
public class OperationSelectionDtoValidator : AbstractValidator<OperationSelectionDto>
{
    public OperationSelectionDtoValidator()
    {
        RuleFor(input => input.SnapshotId).NotEmpty()
            .WithMessage(ConformanceExceptionCodes.SnapshotIdRequired);
        RuleFor(input => input.Method).NotEmpty()
            .When(input => string.IsNullOrWhiteSpace(input.OperationId))
            .WithMessage(ConformanceExceptionCodes.HttpMethodRequired);
        RuleFor(input => input.Path).NotEmpty()
            .When(input => string.IsNullOrWhiteSpace(input.OperationId))
            .WithMessage(ConformanceExceptionCodes.RequestPathRequired);
        RuleFor(input => input.VerbosityCode).Must(SnapshotVerbosityCodes.All.Contains);
    }
}
