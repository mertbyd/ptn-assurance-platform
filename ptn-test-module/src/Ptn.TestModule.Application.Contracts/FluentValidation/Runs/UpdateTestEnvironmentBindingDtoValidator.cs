using System;
using FluentValidation;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.Dtos.Runs;
using Ptn.TestModule.ExceptionCodes.Runs;

namespace Ptn.TestModule.FluentValidation.Runs;

// islevi: Bagli ortamin degistirilebilir hedeflerinin zorunluluk ve uzunluk bicimini dogrular.
// sistemdeki gorevi: Mantiksal anahtar rotadan geldigi icin yalniz hedef alanlarini dogrular.
/// <summary>Bagli test ortaminin guncelleme girdisini dogrular.</summary>
public sealed class UpdateTestEnvironmentBindingDtoValidator : AbstractValidator<UpdateTestEnvironmentBindingDto>
{
    /// <summary>Ortam guncellemesi icin tum public tasima kurallarini kurar.</summary>
    public UpdateTestEnvironmentBindingDtoValidator()
    {
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
        RuleFor(input => input.ApiSecretRef)
            .MaximumLength(TestModuleRunSettingNames.MaxSecretRefLength).WithErrorCode(TestModuleRunErrorCodes.Validation.SecretRefTooLong);
    }
}
