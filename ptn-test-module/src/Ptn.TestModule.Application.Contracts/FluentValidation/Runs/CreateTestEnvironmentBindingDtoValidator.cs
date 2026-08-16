using System;
using FluentValidation;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.Dtos.Runs;
using Ptn.TestModule.ExceptionCodes.Runs;

namespace Ptn.TestModule.FluentValidation.Runs;

// islevi: Yeni ortam baglama girdisinin zorunluluk, uzunluk ve kimlik bicimini dogrular.
// sistemdeki gorevi: Gecersiz public girdiyi setting belgesi okunmadan once durdurur.
/// <summary>Yeni test ortami baglama girdisini dogrular.</summary>
public sealed class CreateTestEnvironmentBindingDtoValidator : AbstractValidator<CreateTestEnvironmentBindingDto>
{
    /// <summary>Yeni ortam baglamasi icin tum public tasima kurallarini kurar.</summary>
    public CreateTestEnvironmentBindingDtoValidator()
    {
        RuleFor(input => input.EnvironmentKey)
            .NotEmpty().WithErrorCode(TestModuleRunErrorCodes.Validation.EnvironmentKeyRequired)
            .MaximumLength(TestRunConsts.MaxEnvironmentKeyLength).WithErrorCode(TestModuleRunErrorCodes.Validation.EnvironmentKeyTooLong);
        RuleFor(input => input.BaseUrl)
            .NotEmpty().WithErrorCode(TestModuleRunErrorCodes.Validation.BaseUrlRequired)
            .MaximumLength(TestModuleRunSettingNames.MaxBaseUrlLength).WithErrorCode(TestModuleRunErrorCodes.Validation.BaseUrlTooLong);
        RuleFor(input => input.SpecSnapshotId)
            .NotEqual(Guid.Empty).WithErrorCode(TestModuleRunErrorCodes.Validation.SpecSnapshotIdInvalid);
        RuleFor(input => input.DbConnectionId)
            .NotEqual(Guid.Empty).WithErrorCode(TestModuleRunErrorCodes.Validation.DbConnectionIdInvalid);
        RuleFor(input => input.SecretRef)
            .NotEmpty().WithErrorCode(TestModuleRunErrorCodes.Validation.SecretRefRequired)
            .MaximumLength(TestModuleRunSettingNames.MaxSecretRefLength).WithErrorCode(TestModuleRunErrorCodes.Validation.SecretRefTooLong);
    }
}
