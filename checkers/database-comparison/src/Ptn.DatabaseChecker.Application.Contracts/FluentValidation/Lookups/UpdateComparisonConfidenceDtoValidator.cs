using Ptn.DatabaseChecker.Dtos.Lookups;

namespace Ptn.DatabaseChecker.FluentValidation.Lookups;

// islevi: Fark guveni (Exact/Canonical/Approximate/Incomparable) update isteginin girdi-format kurallarini calistirir.
// sistemdeki gorevi: Tum kurallar ortak LookupUpdateDtoValidator tabanindan gelir; bu sinif yalnizca DTO tipini baglar.
public class UpdateComparisonConfidenceDtoValidator : LookupUpdateDtoValidator<UpdateComparisonConfidenceDto>
{
}
