using FluentValidation;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Dtos.Authoring;
using Ptn.TestModule.ExceptionCodes.Bridge;

namespace Ptn.TestModule.FluentValidation.Authoring;

// islevi: Profil paketi yukleme girdisinin anahtar ve icerik seklini dogrular.
// sistemdeki gorevi: Bos anahtar veya bos icerigin dosya adi turetmesine ve YAML cozumlemesine ulasmasini engeller.
public sealed class UploadProfilePackDtoValidator : AbstractValidator<UploadProfilePackDto>
{
    public UploadProfilePackDtoValidator()
    {
        RuleFor(input => input.ProfileKey)
            .NotEmpty().WithErrorCode(TestModuleBridgeErrorCodes.Validation.ProfileKeyRequired);
        RuleFor(input => input.Content)
            .NotEmpty().WithErrorCode(TestModuleBridgeErrorCodes.Validation.SourceContentRequired)
            .MaximumLength(PtnBridgeConsts.MaxProfilePackBytes)
            .WithErrorCode(TestModuleBridgeErrorCodes.Validation.SourceContentTooLarge);
    }
}
