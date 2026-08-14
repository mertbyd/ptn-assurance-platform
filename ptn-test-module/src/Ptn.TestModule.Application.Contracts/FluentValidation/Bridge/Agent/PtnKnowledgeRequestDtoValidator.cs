using System.Linq;
using FluentValidation;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Constants.Bridge.Vocabulary;
using Ptn.TestModule.Dtos.Bridge;
using Ptn.TestModule.ExceptionCodes.Bridge;

namespace Ptn.TestModule.FluentValidation.Bridge.Agent;

// islevi: Knowledge girdisinin profil, baglanti, kavram kodu ve response format seklini dogrular.
// sistemdeki gorevi: Profil bilgi yuzeyini serbest soru metni yerine kapali kavram sozlugunde tutar.
public sealed class PtnKnowledgeRequestDtoValidator : AbstractValidator<PtnKnowledgeRequestDto>
{
    public PtnKnowledgeRequestDtoValidator()
    {
        RuleFor(input => input.ProfileKey).NotEmpty().WithMessage(TestModuleBridgeErrorCodes.Validation.ProfileKeyRequired);
        RuleFor(input => input.ConnectionId).NotEmpty().WithMessage(TestModuleBridgeErrorCodes.Validation.ConnectionIdRequired);
        RuleFor(input => input.ConceptCodes).NotEmpty().WithMessage(TestModuleBridgeErrorCodes.Validation.ConceptCodeInvalid);
        RuleForEach(input => input.ConceptCodes).Must(PtnConceptCodes.All.Contains)
            .WithMessage(TestModuleBridgeErrorCodes.Validation.ConceptCodeInvalid);
        RuleFor(input => input.ResponseFormat).Must(PtnResponseFormatCodes.All.Contains)
            .WithMessage(TestModuleBridgeErrorCodes.Validation.ResponseFormatInvalid);
    }
}
