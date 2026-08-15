using System.Text.Json;
using System.Text.RegularExpressions;
using FluentValidation;
using Ptn.TestModule.Dtos.Bridge;
using Ptn.TestModule.ExceptionCodes.Bridge;

namespace Ptn.TestModule.FluentValidation.Bridge.Agent;

// islevi: Overlay onerisi girdisinin bulgu, hedef, gerekce ve JSON seklini dogrular.
// sistemdeki gorevi: Bulgusuz veya gerekcesiz yamanin Manager'a ulasmasini engeller.
public sealed class OverlayPatchRequestDtoValidator : AbstractValidator<OverlayPatchRequestDto>
{
    public OverlayPatchRequestDtoValidator()
    {
        RuleFor(input => input.FindingFingerprint).Matches("^[a-f0-9]{64}$")
            .WithMessage(TestModuleBridgeErrorCodes.Validation.FindingFingerprintInvalid);
        RuleFor(input => input.Target).NotEmpty().WithMessage(TestModuleBridgeErrorCodes.Validation.OverlayTargetRequired);
        RuleFor(input => input.Description).NotEmpty().WithMessage(TestModuleBridgeErrorCodes.Validation.OverlayDescriptionRequired);
        RuleFor(input => input.UpdateJson).Must(IsJsonObject).WithMessage(TestModuleBridgeErrorCodes.Validation.OverlayUpdateInvalid);
    }

    private static bool IsJsonObject(string value)
    {
        try { using var document = JsonDocument.Parse(value); return document.RootElement.ValueKind == JsonValueKind.Object; }
        catch (JsonException) { return false; }
    }
}
