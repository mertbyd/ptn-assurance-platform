using Microsoft.AspNetCore.Mvc;
using Ptn.ApiContractChecker.Constants;
using Ptn.ApiContractChecker.Dtos.Lookups;
using Ptn.ApiContractChecker.Services.Lookups;

namespace Ptn.ApiContractChecker.Controllers.Lookups;

// islevi: DifferenceSeverity lookup yonetimi endpointlerini HTTP uzerinden acar.
// sistemdeki gorevi: Ortak lookup endpoint govdelerine yalniz kararli rota ve servis tipini baglar.
[Route(ApiContractCheckerRoutes.Lookups.DifferenceSeverities)]
public class DifferenceSeverityController
    : LookupControllerBase<IDifferenceSeverityAppService, DifferenceSeverityDto, CreateDifferenceSeverityDto, UpdateDifferenceSeverityDto>
{
}
