using Ptn.DatabaseChecker.Dtos.Lookups;

namespace Ptn.DatabaseChecker.FluentValidation.Lookups;

// islevi: Kapsam kurali turu (Include/Exclude/DataCompare) update isteginin girdi-format kurallarini calistirir.
// sistemdeki gorevi: Tum kurallar ortak LookupUpdateDtoValidator tabanindan gelir; bu sinif yalnizca DTO tipini baglar.
public class UpdateScopeKindDtoValidator : LookupUpdateDtoValidator<UpdateScopeKindDto>
{
}
