using Ptn.DatabaseChecker.Dtos.Lookups;

namespace Ptn.DatabaseChecker.FluentValidation.Lookups;

// islevi: Sema nesne turu (Table/View/Column/...) update isteginin girdi-format kurallarini calistirir.
// sistemdeki gorevi: Tum kurallar ortak LookupUpdateDtoValidator tabanindan gelir; bu sinif yalnizca DTO tipini baglar.
public class UpdateSchemaObjectTypeDtoValidator : LookupUpdateDtoValidator<UpdateSchemaObjectTypeDto>
{
}
