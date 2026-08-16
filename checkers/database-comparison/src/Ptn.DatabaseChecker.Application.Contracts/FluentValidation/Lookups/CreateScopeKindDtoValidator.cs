using Ptn.DatabaseChecker.Dtos.Lookups;

namespace Ptn.DatabaseChecker.FluentValidation.Lookups;

// islevi: Kapsam kurali turu (Include/Exclude/DataCompare) create isteginin girdi-format kurallarini calistirir.
// sistemdeki gorevi: Tum kurallar ortak LookupCreateDtoValidator tabanindan gelir; bu sinif yalnizca DTO tipini baglar.
public class CreateScopeKindDtoValidator : LookupCreateDtoValidator<CreateScopeKindDto>
{
}
