using FluentValidation;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Dtos.Authoring;
using Ptn.TestModule.ExceptionCodes.Bridge;

namespace Ptn.TestModule.FluentValidation.Authoring;

// islevi: Is kurali yukleme girdisinin bos olmamasini ve butce ustu sinirini dogrular.
// sistemdeki gorevi: Bos veya asiri buyuk metnin dosya sinirina ve muhur uretimine ulasmasini engeller.
public sealed class UploadBusinessRulesDtoValidator : AbstractValidator<UploadBusinessRulesDto>
{
    public UploadBusinessRulesDtoValidator()
    {
        RuleFor(input => input.Content)
            .NotEmpty().WithErrorCode(TestModuleBridgeErrorCodes.Validation.SourceContentRequired)
            .MaximumLength(PtnBridgeConsts.MaxBusinessRulesBytes)
            .WithErrorCode(TestModuleBridgeErrorCodes.Validation.SourceContentTooLarge);
    }
}
