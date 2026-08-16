using FluentValidation;
using Ptn.ApiContractChecker.Constants.Conformance.Lookups;
using Ptn.ApiContractChecker.Dtos.Conformance;
using Ptn.ApiContractChecker.ExceptionCodes.Conformance;
using Ptn.ApiContractChecker.FluentValidation.Correlation;

namespace Ptn.ApiContractChecker.FluentValidation.Conformance;

// islevi: Request assertion girdisinin kimlik, operasyon ve profil seklini dogrular.
public class RequestConformanceDtoValidator : AbstractValidator<RequestConformanceDto>
{
    public RequestConformanceDtoValidator()
    {
        RuleFor(input => input.SnapshotId).NotEmpty()
            .WithMessage(ConformanceExceptionCodes.SnapshotIdRequired);
        RuleFor(input => input.Method).NotEmpty()
            .WithMessage(ConformanceExceptionCodes.HttpMethodRequired);
        RuleFor(input => input.Path).NotEmpty()
            .WithMessage(ConformanceExceptionCodes.RequestPathRequired);
        RuleFor(input => input.ProfileCode).Must(ConformanceProfileCodes.All.Contains)
            .WithMessage(ConformanceExceptionCodes.ProfileInvalid);
        RuleFor(input => input.Correlation!)
            .SetValidator(new CorrelationRefDtoValidator())
            .When(input => input.Correlation is not null);
    }
}
