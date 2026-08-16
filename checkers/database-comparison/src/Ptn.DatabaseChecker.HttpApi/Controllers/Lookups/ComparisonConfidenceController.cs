using Microsoft.AspNetCore.Mvc;
using Ptn.DatabaseChecker.Constants;
using Ptn.DatabaseChecker.Dtos.Lookups;
using Ptn.DatabaseChecker.Services.Lookups;

namespace Ptn.DatabaseChecker.Controllers.Lookups;

// islevi: Fark guveni (Exact/Canonical/Approximate/Incomparable) lookup CRUD endpointlerini HTTP uzerinden acar.
// sistemdeki gorevi: Tum endpoint govdeleri LookupControllerBase'ten gelir; bu sinif yalnizca rotayi ve servis tipini baglar.
/// <summary>
/// Fark guveni (Exact/Canonical/Approximate/Incomparable) lookup yonetimi.
/// </summary>
[Route(DatabaseCheckerHttpApiConstants.Routes.ComparisonConfidences)]
[ApiExplorerSettings(GroupName = DatabaseCheckerHttpApiConstants.Groups.Lookups)]
public class ComparisonConfidenceController
    : LookupControllerBase<IComparisonConfidenceAppService, ComparisonConfidenceDto, CreateComparisonConfidenceDto, UpdateComparisonConfidenceDto>
{
}
