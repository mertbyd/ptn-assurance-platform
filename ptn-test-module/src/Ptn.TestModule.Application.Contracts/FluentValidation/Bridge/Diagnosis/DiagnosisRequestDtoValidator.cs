using FluentValidation;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Dtos.Bridge.Diagnosis;
using Ptn.TestModule.ExceptionCodes.Bridge;
using Ptn.TestModule.FluentValidation.Bridge.Database;
using Ptn.TestModule.FluentValidation.Bridge.Correlation;

namespace Ptn.TestModule.FluentValidation.Bridge.Diagnosis;

// islevi: Ortak diagnosis girdisinin ve konum navigation'inin varligini dogrular.
// sistemdeki gorevi: Kaynak-ozgul semantik kararlari Manager'da tutarken tasima seklini korur.
public sealed class DiagnosisRequestDtoValidator : AbstractValidator<DiagnosisRequestDto>
{
    public DiagnosisRequestDtoValidator()
    {
        RuleFor(input => input.Location).NotNull().WithMessage(TestModuleBridgeErrorCodes.Validation.RequestRequired);
        RuleFor(input => input.Location).SetValidator(new LocationDtoValidator());
        RuleForEach(input => input.FailedExpectations).SetValidator(new FailedExpectationDtoValidator());
        RuleFor(input => input.Correlation!)
            .SetValidator(new CorrelationRefDtoValidator())
            .When(input => input.Correlation is not null);

        RuleSet(PtnBridgeValidationRuleSets.Api, () =>
        {
            RuleFor(input => input.SpecSnapshotId).NotNull().NotEqual(Guid.Empty)
                .WithMessage(TestModuleBridgeErrorCodes.Validation.SnapshotIdRequired);
        });

        RuleSet(PtnBridgeValidationRuleSets.Database, () =>
        {
            RuleFor(input => input.ConnectionId).NotEmpty()
                .WithMessage(TestModuleBridgeErrorCodes.Validation.ConnectionIdRequired);
        });
    }
}
