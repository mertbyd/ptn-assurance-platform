using FluentValidation;
using Ptn.DatabaseChecker.Constants;
using Ptn.DatabaseChecker.Dtos.Correlation;
using Ptn.DatabaseChecker.ExceptionCodes;

namespace Ptn.DatabaseChecker.FluentValidation.Correlation;

// islevi: Opsiyonel trace ve adim kimliklerinin public request formatini dogrular.
// sistemdeki gorevi: Iki checker arasindaki korelasyon sozlesmesini ayni uzunluk ve kucuk-harf hex kurallarinda tutar.
public sealed class CorrelationRefDtoValidator : AbstractValidator<CorrelationRefDto>
{
    // islevi: Verilen korelasyon alanlarina kararli format ve uzunluk kurallarini kaydeder.
    public CorrelationRefDtoValidator()
    {
        When(item => item.TraceId is not null, () =>
        {
            RuleFor(item => item.TraceId)
                .Length(CorrelationConsts.TraceIdLength)
                .WithMessage(AssertionExceptionCodes.Validation.CorrelationTraceIdInvalid)
                .Matches(CorrelationConsts.TraceIdPattern)
                .WithMessage(AssertionExceptionCodes.Validation.CorrelationTraceIdInvalid);
        });
        When(item => item.StepKey is not null, () =>
        {
            RuleFor(item => item.StepKey)
                .NotEmpty()
                .WithMessage(AssertionExceptionCodes.Validation.CorrelationStepKeyInvalid)
                .MaximumLength(CorrelationConsts.MaxStepKeyLength)
                .WithMessage(AssertionExceptionCodes.Validation.CorrelationStepKeyInvalid);
        });
    }
}
