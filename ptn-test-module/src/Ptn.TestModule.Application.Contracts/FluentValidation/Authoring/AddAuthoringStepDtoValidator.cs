using System.Linq;
using System.Text;
using System.Text.Json;
using FluentValidation;
using Ptn.TestModule.Constants.Authoring;
using Ptn.TestModule.Dtos.Authoring;
using Ptn.TestModule.ExceptionCodes.Catalog;

namespace Ptn.TestModule.FluentValidation.Authoring;

// islevi: Tek-adim onerisi icin kimlik, JSON body ve assertion pointer bicimlerini dogrular.
// sistemdeki gorevi: Tam belgeyi veya sinirsiz/bozuk model ciktisini domain birlestirmesine sokmaz.
public sealed class AddAuthoringStepDtoValidator : AbstractValidator<AddAuthoringStepDto>
{
    public AddAuthoringStepDtoValidator()
    {
        RuleFor(input => input.StepId)
            .Matches(AuthoringSessionConsts.StepIdPattern)
            .WithErrorCode(TestModuleScenarioErrorCodes.Validation.AuthoringStepIdInvalid);
        RuleFor(input => input.OperationReferenceId)
            .NotEmpty().WithErrorCode(TestModuleScenarioErrorCodes.Validation.AuthoringOperationReferenceRequired);
        RuleFor(input => input.AssertionPaths)
            .NotEmpty().WithErrorCode(TestModuleScenarioErrorCodes.Validation.AuthoringAssertionPathInvalid)
            .Must(paths => paths is not null &&
                           paths.Count <= AuthoringSessionConsts.MaxAssertionsPerStep &&
                           paths.Distinct(System.StringComparer.Ordinal).Count() == paths.Count)
            .WithErrorCode(TestModuleScenarioErrorCodes.Validation.AuthoringAssertionPathInvalid);
        RuleForEach(input => input.AssertionPaths)
            .Matches(AuthoringSessionConsts.JsonPointerPattern)
            .WithErrorCode(TestModuleScenarioErrorCodes.Validation.AuthoringAssertionPathInvalid);
        RuleFor(input => input.RequestBodyJson)
            .Must(IsValidRequestBody)
            .WithErrorCode(TestModuleScenarioErrorCodes.Validation.AuthoringRequestBodyInvalid);
    }

    // Opsiyonel request body'yi butceli ve tek gecerli JSON degeri olarak dogrular.
    private static bool IsValidRequestBody(string? value)
    {
        if (value is null)
        {
            return true;
        }
        if (Encoding.UTF8.GetByteCount(value) > AuthoringSessionConsts.MaxRequestBodyBytes)
        {
            return false;
        }
        try
        {
            using var document = JsonDocument.Parse(value);
            return document.RootElement.ValueKind is JsonValueKind.Object or JsonValueKind.Array;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
