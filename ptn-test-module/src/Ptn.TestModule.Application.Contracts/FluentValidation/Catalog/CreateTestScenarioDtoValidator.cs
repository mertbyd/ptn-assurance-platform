using FluentValidation;
using Ptn.TestModule.Constants.Catalog;
using Ptn.TestModule.Dtos.Catalog;
using Ptn.TestModule.ExceptionCodes.Catalog;

namespace Ptn.TestModule.FluentValidation.Catalog;

// islevi: Senaryo olusturma DTO'sunun zorunlu, uzunluk ve hash bicimlerini dogrular.
// sistemdeki gorevi: Gecersiz public girdiyi Manager ve repository akisi baslamadan durdurur.
public sealed class CreateTestScenarioDtoValidator : AbstractValidator<CreateTestScenarioDto>
{
    public CreateTestScenarioDtoValidator()
    {
        RuleFor(input => input.ScenarioKey)
            .NotEmpty().WithErrorCode(TestModuleScenarioErrorCodes.Validation.ScenarioKeyRequired)
            .Matches(TestScenarioConsts.ScenarioKeyPattern).WithErrorCode(TestModuleScenarioErrorCodes.Validation.ScenarioKeyInvalid);
        RuleFor(input => input.Title)
            .NotEmpty().WithErrorCode(TestModuleScenarioErrorCodes.Validation.TitleRequired)
            .MaximumLength(TestScenarioConsts.MaxTitleLength).WithErrorCode(TestModuleScenarioErrorCodes.Validation.TitleTooLong);
        RuleFor(input => input.Description)
            .MaximumLength(TestScenarioConsts.MaxDescriptionLength).WithErrorCode(TestModuleScenarioErrorCodes.Validation.DescriptionTooLong);
        RuleFor(input => input.SourceDocument)
            .NotEmpty().WithErrorCode(TestModuleScenarioErrorCodes.Validation.SourceDocumentRequired);
        RuleFor(input => input.SourceHash)
            .Matches(TestScenarioConsts.HashPattern).WithErrorCode(TestModuleScenarioErrorCodes.Validation.HashInvalid)
            .When(input => !string.IsNullOrWhiteSpace(input.SourceHash));
        RuleFor(input => input.DerivabilityCode)
            .MaximumLength(TestScenarioConsts.MaxDerivabilityCodeLength).WithErrorCode(TestModuleScenarioErrorCodes.Validation.DerivabilityCodeTooLong);
        RuleFor(input => input.AgentModelRef)
            .MaximumLength(TestScenarioConsts.MaxAgentModelRefLength).WithErrorCode(TestModuleScenarioErrorCodes.Validation.AgentModelRefTooLong);
        RuleFor(input => input.Notes)
            .MaximumLength(TestScenarioConsts.MaxNotesLength).WithErrorCode(TestModuleScenarioErrorCodes.Validation.NotesTooLong);
        RuleFor(input => input.MaterialSeal)
            .NotNull().WithErrorCode(TestModuleScenarioErrorCodes.Validation.MaterialSealRequired)
            .SetValidator(new TestScenarioMaterialSealDtoValidator());
    }
}
