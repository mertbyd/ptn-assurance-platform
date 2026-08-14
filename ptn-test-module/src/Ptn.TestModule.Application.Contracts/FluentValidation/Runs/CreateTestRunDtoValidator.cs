using System;
using FluentValidation;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.Dtos.Runs;
using Ptn.TestModule.ExceptionCodes.Runs;

namespace Ptn.TestModule.FluentValidation.Runs;

// islevi: Yeni kosum DTO'sunun kimlik, zorunluluk, uzunluk ve fingerprint bicimini dogrular.
// sistemdeki gorevi: Gecersiz public girdiyi ortam cozumleme ve Manager akisi baslamadan durdurur.
/// <summary>Yeni test kosumu girdisini dogrular.</summary>
public sealed class CreateTestRunDtoValidator : AbstractValidator<CreateTestRunDto>
{
    /// <summary>Yeni kosum icin tum public tasima kurallarini kurar.</summary>
    public CreateTestRunDtoValidator()
    {
        RuleFor(input => input.ScenarioId)
            .Must(id => !id.HasValue || id.Value != Guid.Empty)
            .WithErrorCode(TestModuleRunErrorCodes.Validation.ScenarioIdInvalid);
        RuleFor(input => input.TestKey)
            .NotEmpty().WithErrorCode(TestModuleRunErrorCodes.Validation.TestKeyRequired)
            .MaximumLength(TestRunConsts.MaxTestKeyLength).WithErrorCode(TestModuleRunErrorCodes.Validation.TestKeyTooLong);
        RuleFor(input => input.EnvironmentKey)
            .NotEmpty().WithErrorCode(TestModuleRunErrorCodes.Validation.EnvironmentKeyRequired)
            .MaximumLength(TestRunConsts.MaxEnvironmentKeyLength).WithErrorCode(TestModuleRunErrorCodes.Validation.EnvironmentKeyTooLong);
        RuleFor(input => input.TriggerKindCode)
            .NotEmpty().WithErrorCode(TestModuleRunErrorCodes.Validation.TriggerKindRequired)
            .MaximumLength(TestResultFindingConsts.MaxKindCodeLength).WithErrorCode(TestModuleRunErrorCodes.Validation.TriggerKindTooLong);
        RuleFor(input => input.TriggerRef)
            .MaximumLength(TestRunConsts.MaxTriggerRefLength).WithErrorCode(TestModuleRunErrorCodes.Validation.TriggerRefTooLong);
        RuleFor(input => input.CanonicalInputs)
            .NotNull().WithErrorCode(TestModuleRunErrorCodes.Validation.CanonicalInputsRequired);
        RuleFor(input => input.SpecFingerprint)
            .Matches(TestRunConsts.FingerprintPattern)
            .When(input => !string.IsNullOrWhiteSpace(input.SpecFingerprint))
            .WithErrorCode(TestModuleRunErrorCodes.Validation.FingerprintInvalid);
        RuleFor(input => input.DbSchemaFingerprint)
            .Matches(TestRunConsts.FingerprintPattern)
            .When(input => !string.IsNullOrWhiteSpace(input.DbSchemaFingerprint))
            .WithErrorCode(TestModuleRunErrorCodes.Validation.FingerprintInvalid);
        RuleFor(input => input.RunnerRef)
            .MaximumLength(TestRunConsts.MaxRunnerRefLength).WithErrorCode(TestModuleRunErrorCodes.Validation.RunnerRefTooLong);
    }
}
