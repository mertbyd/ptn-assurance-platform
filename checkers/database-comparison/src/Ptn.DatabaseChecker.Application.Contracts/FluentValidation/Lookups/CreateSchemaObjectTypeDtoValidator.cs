using Ptn.DatabaseChecker.Dtos.Lookups;

namespace Ptn.DatabaseChecker.FluentValidation.Lookups;

// islevi: Sema nesne turu (Table/View/Column/...) create isteginin girdi-format kurallarini calistirir.
// sistemdeki gorevi: Tum kurallar ortak LookupCreateDtoValidator tabanindan gelir; bu sinif yalnizca DTO tipini baglar.
public class CreateSchemaObjectTypeDtoValidator : LookupCreateDtoValidator<CreateSchemaObjectTypeDto>
{
}
