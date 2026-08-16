using Ptn.DatabaseChecker.Dtos.Lookups;

namespace Ptn.DatabaseChecker.FluentValidation.Lookups;

// islevi: Calistirma durumu (Pending/Running/Completed/Failed) create isteginin girdi-format kurallarini calistirir.
// sistemdeki gorevi: Tum kurallar ortak LookupCreateDtoValidator tabanindan gelir; bu sinif yalnizca DTO tipini baglar.
public class CreateComparisonRunStatusDtoValidator : LookupCreateDtoValidator<CreateComparisonRunStatusDto>
{
}
