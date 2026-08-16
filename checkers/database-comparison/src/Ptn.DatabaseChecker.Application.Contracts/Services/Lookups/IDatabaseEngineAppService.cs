using Ptn.DatabaseChecker.Dtos.Lookups;

namespace Ptn.DatabaseChecker.Services.Lookups;

// islevi: Veritabani motoru (PostgreSql/SqlServer) lookup CRUD kontratini tanimlar.
// sistemdeki gorevi: 5'li CRUD imzalari generic ILookupAppService'ten gelir; bu arayuz yalnizca DTO tiplerini baglar.
public interface IDatabaseEngineAppService : ILookupAppService<DatabaseEngineDto, CreateDatabaseEngineDto, UpdateDatabaseEngineDto>
{
}
