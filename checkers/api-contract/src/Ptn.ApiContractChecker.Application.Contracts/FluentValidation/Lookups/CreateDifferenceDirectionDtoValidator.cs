using Ptn.ApiContractChecker.Dtos.Lookups;

namespace Ptn.ApiContractChecker.FluentValidation.Lookups;

// islevi: DifferenceDirection create isteginin ortak lookup girdi kurallarini calistirir.
// sistemdeki gorevi: Concrete DTO tipini tekrar yazilmayan LookupCreateDtoValidator tabanina baglar.
public class CreateDifferenceDirectionDtoValidator : LookupCreateDtoValidator<CreateDifferenceDirectionDto>
{
}
