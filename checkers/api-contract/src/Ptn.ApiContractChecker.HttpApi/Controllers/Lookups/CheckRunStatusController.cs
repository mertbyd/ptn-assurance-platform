using Microsoft.AspNetCore.Mvc;
using Ptn.ApiContractChecker.Constants;
using Ptn.ApiContractChecker.Dtos.Lookups;
using Ptn.ApiContractChecker.Services.Lookups;

namespace Ptn.ApiContractChecker.Controllers.Lookups;

// islevi: CheckRunStatus lookup yonetimi endpointlerini HTTP uzerinden acar.
// sistemdeki gorevi: Ortak lookup endpoint govdelerine yalniz kararli rota ve servis tipini baglar.
[Route(ApiContractCheckerRoutes.Lookups.CheckRunStatuses)]
public class CheckRunStatusController
    : LookupControllerBase<ICheckRunStatusAppService, CheckRunStatusDto, CreateCheckRunStatusDto, UpdateCheckRunStatusDto>
{
}
