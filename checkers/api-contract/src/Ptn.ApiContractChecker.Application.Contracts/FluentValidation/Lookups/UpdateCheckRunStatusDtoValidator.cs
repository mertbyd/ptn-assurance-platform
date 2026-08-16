using Ptn.ApiContractChecker.Dtos.Lookups;

namespace Ptn.ApiContractChecker.FluentValidation.Lookups;

// islevi: CheckRunStatus update isteginin ortak lookup girdi kurallarini calistirir.
// sistemdeki gorevi: Code icermeyen concrete DTO tipini LookupUpdateDtoValidator tabanina baglar.
public class UpdateCheckRunStatusDtoValidator : LookupUpdateDtoValidator<UpdateCheckRunStatusDto>
{
}
