using Ptn.ApiContractChecker.Dtos.Lookups;

namespace Ptn.ApiContractChecker.FluentValidation.Lookups;

// islevi: SpecFormat create isteginin ortak lookup girdi kurallarini calistirir.
// sistemdeki gorevi: Concrete DTO tipini tekrar yazilmayan LookupCreateDtoValidator tabanina baglar.
public class CreateSpecFormatDtoValidator : LookupCreateDtoValidator<CreateSpecFormatDto>
{
}
