using FluentValidation;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Dtos.Bridge;
using Ptn.TestModule.ExceptionCodes.Bridge;

namespace Ptn.TestModule.FluentValidation.Bridge.Correlation;

// islevi: Opsiyonel trace ve adim anahtarinin checker tel gramerine uydugunu dogrular.
// sistemdeki gorevi: Gecersiz korelasyonun checker cagrisi veya HAR eslestirmesine girmesini engeller.
public sealed class CorrelationRefDtoValidator : AbstractValidator<CorrelationRefDto>
{
    public CorrelationRefDtoValidator()
    {
        When(input => input.TraceId is not null, () =>
        {
            RuleFor(input => input.TraceId!)
                .Length(PtnCorrelationConsts.TraceIdLength)
                .WithMessage(TestModuleBridgeErrorCodes.Validation.CorrelationTraceIdInvalid)
                .Matches(PtnCorrelationConsts.TraceIdPattern)
                .WithMessage(TestModuleBridgeErrorCodes.Validation.CorrelationTraceIdInvalid);
        });
        When(input => input.StepKey is not null, () =>
        {
            RuleFor(input => input.StepKey!)
                .NotEmpty()
                .WithMessage(TestModuleBridgeErrorCodes.Validation.CorrelationStepKeyInvalid)
                .MaximumLength(PtnCorrelationConsts.MaxStepKeyLength)
                .WithMessage(TestModuleBridgeErrorCodes.Validation.CorrelationStepKeyInvalid);
        });
    }
}
