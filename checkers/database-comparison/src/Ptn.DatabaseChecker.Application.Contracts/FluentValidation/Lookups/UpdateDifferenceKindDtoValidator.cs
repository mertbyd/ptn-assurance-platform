using Ptn.DatabaseChecker.Dtos.Lookups;

namespace Ptn.DatabaseChecker.FluentValidation.Lookups;

// islevi: Fark yonu (OnlyInSource/OnlyInTarget/Modified) update isteginin girdi-format kurallarini calistirir.
// sistemdeki gorevi: Tum kurallar ortak LookupUpdateDtoValidator tabanindan gelir; bu sinif yalnizca DTO tipini baglar.
public class UpdateDifferenceKindDtoValidator : LookupUpdateDtoValidator<UpdateDifferenceKindDto>
{
}
