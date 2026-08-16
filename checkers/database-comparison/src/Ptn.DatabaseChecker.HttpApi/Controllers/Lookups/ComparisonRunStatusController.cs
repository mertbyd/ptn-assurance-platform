using Microsoft.AspNetCore.Mvc;
using Ptn.DatabaseChecker.Constants;
using Ptn.DatabaseChecker.Dtos.Lookups;
using Ptn.DatabaseChecker.Services.Lookups;

namespace Ptn.DatabaseChecker.Controllers.Lookups;

// islevi: Calistirma durumu (Pending/Running/Completed/Failed) lookup CRUD endpointlerini HTTP uzerinden acar.
// sistemdeki gorevi: Tum endpoint govdeleri LookupControllerBase'ten gelir; bu sinif yalnizca rotayi ve servis tipini baglar.
/// <summary>
/// Calistirma durumu (Pending/Running/Completed/Failed) lookup yonetimi.
/// </summary>
[Route(DatabaseCheckerHttpApiConstants.Routes.ComparisonRunStatuses)]
[ApiExplorerSettings(GroupName = DatabaseCheckerHttpApiConstants.Groups.Lookups)]
public class ComparisonRunStatusController
    : LookupControllerBase<IComparisonRunStatusAppService, ComparisonRunStatusDto, CreateComparisonRunStatusDto, UpdateComparisonRunStatusDto>
{
}
