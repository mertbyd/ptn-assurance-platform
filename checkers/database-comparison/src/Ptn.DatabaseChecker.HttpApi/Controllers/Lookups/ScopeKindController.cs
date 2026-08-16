using Microsoft.AspNetCore.Mvc;
using Ptn.DatabaseChecker.Constants;
using Ptn.DatabaseChecker.Dtos.Lookups;
using Ptn.DatabaseChecker.Services.Lookups;

namespace Ptn.DatabaseChecker.Controllers.Lookups;

// islevi: Kapsam kurali turu (Include/Exclude/DataCompare) lookup CRUD endpointlerini HTTP uzerinden acar.
// sistemdeki gorevi: Tum endpoint govdeleri LookupControllerBase'ten gelir; bu sinif yalnizca rotayi ve servis tipini baglar.
/// <summary>
/// Kapsam kurali turu (Include/Exclude/DataCompare) lookup yonetimi.
/// </summary>
[Route(DatabaseCheckerHttpApiConstants.Routes.ScopeKinds)]
[ApiExplorerSettings(GroupName = DatabaseCheckerHttpApiConstants.Groups.Lookups)]
public class ScopeKindController
    : LookupControllerBase<IScopeKindAppService, ScopeKindDto, CreateScopeKindDto, UpdateScopeKindDto>
{
}
