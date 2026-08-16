using Ptn.DatabaseChecker.Dtos.Lookups;

namespace Ptn.DatabaseChecker.FluentValidation.Lookups;

// islevi: Veritabani motoru (PostgreSql/SqlServer) update isteginin girdi-format kurallarini calistirir.
// sistemdeki gorevi: Tum kurallar ortak LookupUpdateDtoValidator tabanindan gelir; bu sinif yalnizca DTO tipini baglar.
public class UpdateDatabaseEngineDtoValidator : LookupUpdateDtoValidator<UpdateDatabaseEngineDto>
{
}
