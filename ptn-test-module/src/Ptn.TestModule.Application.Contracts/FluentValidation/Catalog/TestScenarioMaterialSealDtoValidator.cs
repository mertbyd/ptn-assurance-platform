using System;
using FluentValidation;
using Ptn.TestModule.Constants.Catalog;
using Ptn.TestModule.Dtos.Catalog;
using Ptn.TestModule.ExceptionCodes.Catalog;

namespace Ptn.TestModule.FluentValidation.Catalog;

// islevi: Opsiyonel malzeme kimligi ve SHA-256 alanlarinin tasima bicimini dogrular.
// sistemdeki gorevi: Eksik muhrun gate karari olarak kalmasini, gecersiz bicimin sinirda durmasini saglar.
public sealed class TestScenarioMaterialSealDtoValidator : AbstractValidator<TestScenarioMaterialSealDto>
{
    public TestScenarioMaterialSealDtoValidator()
    {
        RuleFor(input => input.RulesFingerprint)
            .Matches(TestScenarioConsts.HashPattern)
            .When(input => !string.IsNullOrWhiteSpace(input.RulesFingerprint))
            .WithErrorCode(TestModuleScenarioErrorCodes.Validation.HashInvalid);
        RuleFor(input => input.SpecFingerprint)
            .Matches(TestScenarioConsts.HashPattern)
            .When(input => !string.IsNullOrWhiteSpace(input.SpecFingerprint))
            .WithErrorCode(TestModuleScenarioErrorCodes.Validation.HashInvalid);
        RuleFor(input => input.DbSchemaFingerprint)
            .Matches(TestScenarioConsts.HashPattern)
            .When(input => !string.IsNullOrWhiteSpace(input.DbSchemaFingerprint))
            .WithErrorCode(TestModuleScenarioErrorCodes.Validation.HashInvalid);
        RuleFor(input => input.ProfileFingerprint)
            .Matches(TestScenarioConsts.HashPattern)
            .When(input => !string.IsNullOrWhiteSpace(input.ProfileFingerprint))
            .WithErrorCode(TestModuleScenarioErrorCodes.Validation.HashInvalid);
        RuleFor(input => input.SpecSnapshotId)
            .Must(id => !id.HasValue || id.Value != Guid.Empty)
            .WithErrorCode(TestModuleScenarioErrorCodes.Validation.MaterialIdentityInvalid);
        RuleFor(input => input.DbConnectionId)
            .Must(id => !id.HasValue || id.Value != Guid.Empty)
            .WithErrorCode(TestModuleScenarioErrorCodes.Validation.MaterialIdentityInvalid);
    }
}
