using Microsoft.AspNetCore.Mvc;
using Ptn.DatabaseChecker.Constants;
using Ptn.DatabaseChecker.Dtos.Lookups;
using Ptn.DatabaseChecker.Services.Lookups;

namespace Ptn.DatabaseChecker.Controllers.Lookups;

// islevi: Veritabani motoru (PostgreSql/SqlServer) lookup CRUD endpointlerini HTTP uzerinden acar.
// sistemdeki gorevi: Tum endpoint govdeleri LookupControllerBase'ten gelir; bu sinif yalnizca rotayi ve servis tipini baglar.
/// <summary>
/// Veritabani motoru (PostgreSql/SqlServer) lookup yonetimi.
/// </summary>
[Route(DatabaseCheckerHttpApiConstants.Routes.DatabaseEngines)]
[ApiExplorerSettings(GroupName = DatabaseCheckerHttpApiConstants.Groups.Lookups)]
public class DatabaseEngineController
    : LookupControllerBase<IDatabaseEngineAppService, DatabaseEngineDto, CreateDatabaseEngineDto, UpdateDatabaseEngineDto>
{
}
