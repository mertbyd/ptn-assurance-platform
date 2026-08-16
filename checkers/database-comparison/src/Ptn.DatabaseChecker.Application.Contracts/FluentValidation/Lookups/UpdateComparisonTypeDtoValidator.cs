using Ptn.DatabaseChecker.Dtos.Lookups;

namespace Ptn.DatabaseChecker.FluentValidation.Lookups;

// islevi: Karsilastirma modu (SchemaOnly/DataOnly/Both) update isteginin girdi-format kurallarini calistirir.
// sistemdeki gorevi: Tum kurallar ortak LookupUpdateDtoValidator tabanindan gelir; bu sinif yalnizca DTO tipini baglar.
public class UpdateComparisonTypeDtoValidator : LookupUpdateDtoValidator<UpdateComparisonTypeDto>
{
}
