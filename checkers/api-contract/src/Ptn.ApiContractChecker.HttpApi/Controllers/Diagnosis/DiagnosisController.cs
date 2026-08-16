using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ptn.ApiContractChecker.Constants;
using Ptn.ApiContractChecker.Dtos.Diagnosis;
using Ptn.ApiContractChecker.Permissions;
using Ptn.ApiContractChecker.Services.Diagnosis;
using SystemStandards.Results;

namespace Ptn.ApiContractChecker.Controllers.Diagnosis;

// islevi: Dinamik API teshis use-case'ini tek HTTP POST endpoint'iyle acar.
// sistemdeki gorevi: Yetkilendirilmis request'i is karari tasimadan IDiagnosisAppService'e yonlendirir.
/// <summary>API hata teshis islemleri.</summary>
[Route(ApiContractCheckerRoutes.Diagnosis)]
[ApiExplorerSettings(GroupName = ApiContractCheckerSwaggerConstants.DiagnosisGroupName)]
[Authorize(ApiContractCheckerPermissions.Diagnosis.Execute)]
public class DiagnosisController : ApiContractCheckerController
{
    private IDiagnosisAppService AppService => LazyGetRequiredService<IDiagnosisAppService>();

    /// <summary>Basarisiz API adimi icin deterministik teshis raporu uretir.</summary>
    [HttpPost]
    public async Task<Result<DiagnosisReportDto>> Diagnose(DiagnoseRequestDto input)
    {
        var result = await AppService.DiagnoseAsync(input);
        return result;
    }
}
