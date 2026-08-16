using FluentValidation;
using Ptn.ApiContractChecker.Dtos.Sources;
using Ptn.ApiContractChecker.ExceptionCodes.Sources;

namespace Ptn.ApiContractChecker.FluentValidation.Sources;

// islevi: SpecSource create istegine ortak kurallarin yaninda sunucu-tarafli dokuman kimligi kuralini uygular.
// sistemdeki gorevi: Yeni aggregate cocuk kimliklerinin istemciden alinmasini engeller; kimlikleri ABP GuidGenerator uretir.
public class CreateSpecSourceDtoValidator : SpecSourceDtoValidatorBase<CreateSpecSourceDto>
{
    public CreateSpecSourceDtoValidator()
    {
        // Create komutunda dokuman kimligi istemciden gelemez.
        RuleForEach(source => source.Documents)
            .Must(document => document.Id == Guid.Empty)
            .WithErrorCode(SpecSourceExceptionCodes.Validation.DocumentIdMustBeEmpty);
    }
}
