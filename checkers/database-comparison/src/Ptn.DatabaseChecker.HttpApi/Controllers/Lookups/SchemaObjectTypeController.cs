using Microsoft.AspNetCore.Mvc;
using Ptn.DatabaseChecker.Constants;
using Ptn.DatabaseChecker.Dtos.Lookups;
using Ptn.DatabaseChecker.Services.Lookups;

namespace Ptn.DatabaseChecker.Controllers.Lookups;

// islevi: Sema nesne turu (Table/View/Column/...) lookup CRUD endpointlerini HTTP uzerinden acar.
// sistemdeki gorevi: Tum endpoint govdeleri LookupControllerBase'ten gelir; bu sinif yalnizca rotayi ve servis tipini baglar.
/// <summary>
/// Sema nesne turu (Table/View/Column/...) lookup yonetimi.
/// </summary>
[Route(DatabaseCheckerHttpApiConstants.Routes.SchemaObjectTypes)]
[ApiExplorerSettings(GroupName = DatabaseCheckerHttpApiConstants.Groups.Lookups)]
public class SchemaObjectTypeController
    : LookupControllerBase<ISchemaObjectTypeAppService, SchemaObjectTypeDto, CreateSchemaObjectTypeDto, UpdateSchemaObjectTypeDto>
{
}
