using FluentValidation;
using Ptn.ApiContractChecker.Dtos.Diagnosis;
using Ptn.ApiContractChecker.ExceptionCodes.Diagnosis;

namespace Ptn.ApiContractChecker.FluentValidation.Diagnosis;

// islevi: Problem error girdisinin pointer veya koddan en az birini tasimasini dogrular.
// sistemdeki gorevi: Bos yapilandirilmis error satirlarini domain kimlik cikarimina girmeden reddeder.
public sealed class ProblemErrorDtoValidator : AbstractValidator<ProblemErrorDto>
{
    public ProblemErrorDtoValidator()
    {
        RuleFor(input => input)
            .Must(input => !string.IsNullOrWhiteSpace(input.Pointer) || !string.IsNullOrWhiteSpace(input.Code))
            .WithErrorCode(DiagnosisExceptionCodes.ProblemErrorInvalid);
    }
}
