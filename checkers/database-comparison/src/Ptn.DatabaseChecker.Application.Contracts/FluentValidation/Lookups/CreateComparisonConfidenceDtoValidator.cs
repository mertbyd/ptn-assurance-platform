using Ptn.DatabaseChecker.Dtos.Lookups;

namespace Ptn.DatabaseChecker.FluentValidation.Lookups;

// islevi: Fark guveni (Exact/Canonical/Approximate/Incomparable) create isteginin girdi-format kurallarini calistirir.
// sistemdeki gorevi: Tum kurallar ortak LookupCreateDtoValidator tabanindan gelir; bu sinif yalnizca DTO tipini baglar.
public class CreateComparisonConfidenceDtoValidator : LookupCreateDtoValidator<CreateComparisonConfidenceDto>
{
}
