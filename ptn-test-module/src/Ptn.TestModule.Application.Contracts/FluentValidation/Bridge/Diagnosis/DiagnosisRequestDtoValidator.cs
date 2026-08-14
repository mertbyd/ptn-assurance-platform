using FluentValidation;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Dtos.Bridge.Diagnosis;
using Ptn.TestModule.ExceptionCodes.Bridge;
using Ptn.TestModule.FluentValidation.Bridge.Database;

namespace Ptn.TestModule.FluentValidation.Bridge.Diagnosis;

// islevi: Ortak diagnosis girdisinin ve konum navigation'inin varligini dogrular.
// sistemdeki gorevi: Kaynak-ozgul semantik kararlari Manager'da tutarken tasima seklini korur.
public sealed class DiagnosisRequestDtoValidator : AbstractValidator<DiagnosisRequestDto>
{
    public DiagnosisRequestDtoValidator()
    {
        RuleFor(input => input.Location).NotNull().WithErrorCode(TestModuleBridgeValidationErrorCodes.RequestRequired);
        RuleFor(input => input.Location).SetValidator(new LocationDtoValidator());
        RuleForEach(input => input.FailedExpectations).SetValidator(new FailedExpectationDtoValidator());

        RuleSet(PtnBridgeValidationRuleSets.Api, () =>
        {
            RuleFor(input => input.SpecSnapshotId).NotNull().NotEqual(Guid.Empty)
                .WithErrorCode(TestModuleBridgeValidationErrorCodes.SnapshotIdRequired);
        });

        RuleSet(PtnBridgeValidationRuleSets.Database, () =>
        {
            RuleFor(input => input.ConnectionId).NotEmpty()
                .WithErrorCode(TestModuleBridgeValidationErrorCodes.ConnectionIdRequired);
        });
    }
}
