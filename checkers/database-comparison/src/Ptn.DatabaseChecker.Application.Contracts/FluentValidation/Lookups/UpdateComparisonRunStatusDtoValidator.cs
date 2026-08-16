using Ptn.DatabaseChecker.Dtos.Lookups;

namespace Ptn.DatabaseChecker.FluentValidation.Lookups;

// islevi: Calistirma durumu (Pending/Running/Completed/Failed) update isteginin girdi-format kurallarini calistirir.
// sistemdeki gorevi: Tum kurallar ortak LookupUpdateDtoValidator tabanindan gelir; bu sinif yalnizca DTO tipini baglar.
public class UpdateComparisonRunStatusDtoValidator : LookupUpdateDtoValidator<UpdateComparisonRunStatusDto>
{
}
