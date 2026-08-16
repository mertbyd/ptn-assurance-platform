using FluentValidation;
using Ptn.ApiContractChecker.Dtos.Diagnosis;
using Ptn.ApiContractChecker.ExceptionCodes.Diagnosis;
using Ptn.ApiContractChecker.FluentValidation.Correlation;

namespace Ptn.ApiContractChecker.FluentValidation.Diagnosis;

// islevi: Diagnosis request snapshot, operation, status ve nested problem error sekil kurallarini dogrular.
// sistemdeki gorevi: Veritabani ve hipotez kararlarini manager'a birakip HTTP sinirinda yalniz request seklini korur.
public sealed class DiagnoseRequestDtoValidator : AbstractValidator<DiagnoseRequestDto>
{
    public DiagnoseRequestDtoValidator(ProblemErrorDtoValidator problemErrorValidator)
    {
        RuleFor(input => input.SnapshotId)
            .NotEmpty().WithErrorCode(DiagnosisExceptionCodes.SnapshotIdRequired);
        RuleFor(input => input.Method)
            .NotEmpty().WithErrorCode(DiagnosisExceptionCodes.HttpMethodRequired);
        RuleFor(input => input.Path)
            .NotEmpty().WithErrorCode(DiagnosisExceptionCodes.RequestPathRequired);
        RuleFor(input => input.StatusCode)
            .InclusiveBetween(100, 599)
            .When(input => input.StatusCode.HasValue)
            .WithErrorCode(DiagnosisExceptionCodes.StatusCodeInvalid);
        RuleForEach(input => input.ProblemErrors).SetValidator(problemErrorValidator);
        RuleFor(input => input.Correlation!)
            .SetValidator(new CorrelationRefDtoValidator())
            .When(input => input.Correlation is not null);
    }
}
