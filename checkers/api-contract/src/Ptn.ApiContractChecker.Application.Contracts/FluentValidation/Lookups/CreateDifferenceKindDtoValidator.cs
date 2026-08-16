using Ptn.ApiContractChecker.Dtos.Lookups;

namespace Ptn.ApiContractChecker.FluentValidation.Lookups;

// islevi: DifferenceKind create isteginin ortak lookup girdi kurallarini calistirir.
// sistemdeki gorevi: Concrete DTO tipini tekrar yazilmayan LookupCreateDtoValidator tabanina baglar.
public class CreateDifferenceKindDtoValidator : LookupCreateDtoValidator<CreateDifferenceKindDto>
{
}
