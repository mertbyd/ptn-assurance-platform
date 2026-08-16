using FluentValidation;
using Ptn.DatabaseChecker.Constants.Lookups;
using Ptn.DatabaseChecker.Dtos.Lookups;
using Ptn.DatabaseChecker.ExceptionCodes;

namespace Ptn.DatabaseChecker.FluentValidation.Lookups;

// islevi: Tum lookup create DTO'lari icin ortak girdi-format kurallarini (bos/uzunluk) tanimlar.
// sistemdeki gorevi: Her lookup icin ayni Code/Name/Description kurallarinin tekrar yazilmasini engeller; concrete validator sadece tipi baglar (golden rule 1: is bir kez yapilir).
public abstract class LookupCreateDtoValidator<TDto> : AbstractValidator<TDto>
    where TDto : LookupCreateDto
{
    protected LookupCreateDtoValidator()
    {
        // Code bos olamaz ve lookup kod uzunlugunu asamaz (sema ile ayni sabit).
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage(LookupExceptionCodes.Validation.CodeRequired)
            .MaximumLength(LookupConsts.MaxCodeLength).WithMessage(LookupExceptionCodes.Validation.CodeMaxLength);

        // Name bos olamaz ve ad uzunlugunu asamaz.
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(LookupExceptionCodes.Validation.NameRequired)
            .MaximumLength(LookupConsts.MaxNameLength).WithMessage(LookupExceptionCodes.Validation.NameMaxLength);

        // Description opsiyonel; verilirse uzunluk sinirini asamaz.
        RuleFor(x => x.Description)
            .MaximumLength(LookupConsts.MaxDescriptionLength).WithMessage(LookupExceptionCodes.Validation.DescriptionMaxLength);
    }
}
