using Ptn.ApiContractChecker.Dtos.Lookups;

namespace Ptn.ApiContractChecker.FluentValidation.Lookups;

// islevi: DifferenceSeverity create isteginin ortak lookup girdi kurallarini calistirir.
// sistemdeki gorevi: Concrete DTO tipini tekrar yazilmayan LookupCreateDtoValidator tabanina baglar.
public class CreateDifferenceSeverityDtoValidator : LookupCreateDtoValidator<CreateDifferenceSeverityDto>
{
}
