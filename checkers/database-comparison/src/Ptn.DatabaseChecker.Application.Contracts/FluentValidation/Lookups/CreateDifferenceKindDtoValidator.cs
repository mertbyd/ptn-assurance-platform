using Ptn.DatabaseChecker.Dtos.Lookups;

namespace Ptn.DatabaseChecker.FluentValidation.Lookups;

// islevi: Fark yonu (OnlyInSource/OnlyInTarget/Modified) create isteginin girdi-format kurallarini calistirir.
// sistemdeki gorevi: Tum kurallar ortak LookupCreateDtoValidator tabanindan gelir; bu sinif yalnizca DTO tipini baglar.
public class CreateDifferenceKindDtoValidator : LookupCreateDtoValidator<CreateDifferenceKindDto>
{
}
