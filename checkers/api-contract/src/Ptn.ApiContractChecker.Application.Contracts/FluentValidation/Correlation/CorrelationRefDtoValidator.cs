using FluentValidation;
using Ptn.ApiContractChecker.Constants;
using Ptn.ApiContractChecker.Dtos.Correlation;
using Ptn.ApiContractChecker.ExceptionCodes;

namespace Ptn.ApiContractChecker.FluentValidation.Correlation;

// islevi: Opsiyonel korelasyon trace ve adim anahtarinin public request seklini dogrular.
// sistemdeki gorevi: Checker'lar arasi ikiz korelasyon sozlesmesini kararli kodlarla korur.
public sealed class CorrelationRefDtoValidator : AbstractValidator<CorrelationRefDto>
{
    public CorrelationRefDtoValidator()
    {
        RuleFor(input => input.TraceId)
            .Length(CorrelationConsts.TraceIdLength)
            .WithErrorCode(GeneralExceptionCodes.CorrelationTraceIdInvalid)
            .Matches(CorrelationConsts.TraceIdPattern)
            .WithErrorCode(GeneralExceptionCodes.CorrelationTraceIdInvalid)
            .When(input => input.TraceId is not null);
        RuleFor(input => input.StepKey)
            .NotEmpty()
            .WithErrorCode(GeneralExceptionCodes.CorrelationStepKeyInvalid)
            .MaximumLength(CorrelationConsts.MaxStepKeyLength)
            .WithErrorCode(GeneralExceptionCodes.CorrelationStepKeyInvalid)
            .Must(stepKey => !string.IsNullOrWhiteSpace(stepKey))
            .WithErrorCode(GeneralExceptionCodes.CorrelationStepKeyInvalid)
            .When(input => input.StepKey is not null);
    }
}
