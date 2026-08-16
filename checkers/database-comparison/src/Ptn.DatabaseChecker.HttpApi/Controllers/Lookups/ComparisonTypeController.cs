using Microsoft.AspNetCore.Mvc;
using Ptn.DatabaseChecker.Constants;
using Ptn.DatabaseChecker.Dtos.Lookups;
using Ptn.DatabaseChecker.Services.Lookups;

namespace Ptn.DatabaseChecker.Controllers.Lookups;

// islevi: Karsilastirma modu (SchemaOnly/DataOnly/Both) lookup CRUD endpointlerini HTTP uzerinden acar.
// sistemdeki gorevi: Tum endpoint govdeleri LookupControllerBase'ten gelir; bu sinif yalnizca rotayi ve servis tipini baglar.
/// <summary>
/// Karsilastirma modu (SchemaOnly/DataOnly/Both) lookup yonetimi.
/// </summary>
[Route(DatabaseCheckerHttpApiConstants.Routes.ComparisonTypes)]
[ApiExplorerSettings(GroupName = DatabaseCheckerHttpApiConstants.Groups.Lookups)]
public class ComparisonTypeController
    : LookupControllerBase<IComparisonTypeAppService, ComparisonTypeDto, CreateComparisonTypeDto, UpdateComparisonTypeDto>
{
}
