using Ptn.ApiContractChecker.Dtos.Sources;

namespace Ptn.ApiContractChecker.FluentValidation.Sources;

// islevi: SpecSource update istegini ortak alan, dokuman ve all-or-nothing credential kurallarindan gecirir.
// sistemdeki gorevi: Id'si bos dokumani ekleme, dolu dokumani aggregate uzerinden guncelleme niyeti olarak korur.
public class UpdateSpecSourceDtoValidator : SpecSourceDtoValidatorBase<UpdateSpecSourceDto>
{
}
