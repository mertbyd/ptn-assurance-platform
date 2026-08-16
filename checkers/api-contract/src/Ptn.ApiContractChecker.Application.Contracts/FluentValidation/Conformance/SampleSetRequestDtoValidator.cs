using FluentValidation;
using Ptn.ApiContractChecker.Constants.Conformance;
using Ptn.ApiContractChecker.Constants.Conformance.Lookups;
using Ptn.ApiContractChecker.Dtos.Conformance;
using Ptn.ApiContractChecker.ExceptionCodes.Conformance;

namespace Ptn.ApiContractChecker.FluentValidation.Conformance;

// islevi: Sample set isteginin snapshot, operasyon secimi, kapali turu ve alan butcesini dogrular.
// sistemdeki gorevi: Manager'a yalniz sekil olarak gecerli ve sinirli public input ulasmasini saglar.
public class SampleSetRequestDtoValidator : AbstractValidator<SampleSetRequestDto>
{
    public SampleSetRequestDtoValidator()
    {
        RuleFor(input => input.SnapshotId).NotEmpty()
            .WithErrorCode(ConformanceExceptionCodes.SnapshotIdRequired);
        RuleFor(input => input.Method).NotEmpty()
            .When(input => string.IsNullOrWhiteSpace(input.OperationId))
            .WithErrorCode(ConformanceExceptionCodes.HttpMethodRequired);
        RuleFor(input => input.Path).NotEmpty()
            .When(input => string.IsNullOrWhiteSpace(input.OperationId))
            .WithErrorCode(ConformanceExceptionCodes.RequestPathRequired);
        RuleFor(input => input.SampleKindCode).Must(SampleKindCodes.All.Contains)
            .WithErrorCode(ConformanceExceptionCodes.SampleKindInvalid);
        RuleFor(input => input.MaxSamplesPerField)
            .InclusiveBetween(1, SampleGenerationConsts.MaxSamplesPerField)
            .WithErrorCode(ConformanceExceptionCodes.MaxSamplesPerFieldInvalid);
    }
}
