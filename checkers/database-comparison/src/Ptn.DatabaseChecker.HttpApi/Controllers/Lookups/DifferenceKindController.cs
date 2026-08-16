using Microsoft.AspNetCore.Mvc;
using Ptn.DatabaseChecker.Constants;
using Ptn.DatabaseChecker.Dtos.Lookups;
using Ptn.DatabaseChecker.Services.Lookups;

namespace Ptn.DatabaseChecker.Controllers.Lookups;

// islevi: Fark yonu (OnlyInSource/OnlyInTarget/Modified) lookup CRUD endpointlerini HTTP uzerinden acar.
// sistemdeki gorevi: Tum endpoint govdeleri LookupControllerBase'ten gelir; bu sinif yalnizca rotayi ve servis tipini baglar.
/// <summary>
/// Fark yonu (OnlyInSource/OnlyInTarget/Modified) lookup yonetimi.
/// </summary>
[Route(DatabaseCheckerHttpApiConstants.Routes.DifferenceKinds)]
[ApiExplorerSettings(GroupName = DatabaseCheckerHttpApiConstants.Groups.Lookups)]
public class DifferenceKindController
    : LookupControllerBase<IDifferenceKindAppService, DifferenceKindDto, CreateDifferenceKindDto, UpdateDifferenceKindDto>
{
}
