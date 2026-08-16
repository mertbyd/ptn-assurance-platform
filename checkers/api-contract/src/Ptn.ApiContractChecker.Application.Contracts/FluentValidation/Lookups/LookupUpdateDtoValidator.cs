using FluentValidation;
using Ptn.ApiContractChecker.Constants.Lookups;
using Ptn.ApiContractChecker.Dtos.Lookups;
using Ptn.ApiContractChecker.ExceptionCodes;

namespace Ptn.ApiContractChecker.FluentValidation.Lookups;

// islevi: Tum lookup update DTO'lari icin degisebilir Name/Description alanlarinin ortak girdi-format kurallarini tanimlar.
// sistemdeki gorevi: Kararli Code'u update sozlesmesinden uzak tutarken gorunen alan kurallarinin tekrar yazilmasini engeller.
public abstract class LookupUpdateDtoValidator<TDto> : AbstractValidator<TDto>
    where TDto : LookupUpdateDto
{
    protected LookupUpdateDtoValidator()
    {
        // Name bos olamaz ve ad uzunlugunu asamaz.
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(LookupExceptionCodes.Validation.NameRequired)
            .MaximumLength(LookupConsts.MaxNameLength).WithMessage(LookupExceptionCodes.Validation.NameMaxLength);

        // Description opsiyonel; verilirse uzunluk sinirini asamaz.
        RuleFor(x => x.Description)
            .MaximumLength(LookupConsts.MaxDescriptionLength).WithMessage(LookupExceptionCodes.Validation.DescriptionMaxLength);
    }
}
