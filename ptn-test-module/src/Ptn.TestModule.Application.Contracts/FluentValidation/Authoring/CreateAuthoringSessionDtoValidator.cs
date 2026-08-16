using System;
using FluentValidation;
using Ptn.TestModule.Constants.Authoring;
using Ptn.TestModule.Dtos.Authoring;
using Ptn.TestModule.ExceptionCodes.Catalog;
using Ptn.TestModule.FluentValidation.Bridge.Agent;

namespace Ptn.TestModule.FluentValidation.Authoring;

// islevi: Oturum baslangicinin grounding, workflow ve sourceDescription seklini dogrular.
// sistemdeki gorevi: Checker snapshot'iyla ilgisiz veya gecersiz URL'li oturumu sinirda durdurur.
public sealed class CreateAuthoringSessionDtoValidator : AbstractValidator<CreateAuthoringSessionDto>
{
    public CreateAuthoringSessionDtoValidator()
    {
        RuleFor(input => input.Grounding)
            .NotNull().WithErrorCode(TestModuleScenarioErrorCodes.Validation.AuthoringGroundingRequired)
            .SetValidator(new GroundRequestDtoValidator());
        RuleFor(input => input.WorkflowId)
            .Matches(AuthoringSessionConsts.WorkflowIdPattern)
            .WithErrorCode(TestModuleScenarioErrorCodes.Validation.AuthoringWorkflowIdInvalid);
        RuleFor(input => input.WorkflowSummary)
            .NotEmpty().WithErrorCode(TestModuleScenarioErrorCodes.Validation.AuthoringWorkflowSummaryRequired)
            .MaximumLength(AuthoringSessionConsts.MaxWorkflowSummaryLength)
            .WithErrorCode(TestModuleScenarioErrorCodes.Validation.AuthoringWorkflowSummaryRequired);
        RuleFor(input => input.ApiSourceUrl)
            .Must((input, value) => input.Grounding is not null &&
                IsSourceUrl(value) && value.Contains(
                input.Grounding.SpecSnapshotId.ToString("D"), StringComparison.OrdinalIgnoreCase))
            .WithErrorCode(TestModuleScenarioErrorCodes.Validation.AuthoringSourceUrlInvalid);
        RuleFor(input => input.DatabaseSourceUrl)
            .Must(IsSourceUrl)
            .WithErrorCode(TestModuleScenarioErrorCodes.Validation.AuthoringSourceUrlInvalid);
    }

    // SourceDescription URL'sini mutlak veya dosya-goreli, butceli bir URI olarak dogrular.
    private static bool IsSourceUrl(string? value) =>
        !string.IsNullOrWhiteSpace(value) &&
        value.Length <= AuthoringSessionConsts.MaxSourceUrlLength &&
        Uri.TryCreate(value, UriKind.RelativeOrAbsolute, out _);
}
