using Microsoft.AspNetCore.Mvc;
using Ptn.ApiContractChecker.Constants;
using Ptn.ApiContractChecker.Dtos.Lookups;
using Ptn.ApiContractChecker.Services.Lookups;

namespace Ptn.ApiContractChecker.Controllers.Lookups;

// islevi: DifferenceKind lookup yonetimi endpointlerini HTTP uzerinden acar.
// sistemdeki gorevi: Ortak lookup endpoint govdelerine yalniz kararli rota ve servis tipini baglar.
[Route(ApiContractCheckerRoutes.Lookups.DifferenceKinds)]
public class DifferenceKindController
    : LookupControllerBase<IDifferenceKindAppService, DifferenceKindDto, CreateDifferenceKindDto, UpdateDifferenceKindDto>
{
}
