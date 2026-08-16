using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ptn.ApiContractChecker.Constants;
using Ptn.ApiContractChecker.Dtos.Runs;
using Ptn.ApiContractChecker.Dtos.Runs.Reports;
using Ptn.ApiContractChecker.Permissions;
using Ptn.ApiContractChecker.Services.Runs;
using SystemStandards.Results;

namespace Ptn.ApiContractChecker.Controllers.Runs;

// islevi: Contract check tetikleme, gecmis, detay, status ve rapor endpointlerini HTTP uzerinden acar.
// sistemdeki gorevi: Execute izniyle yalniz sistem-yazimli Pending run tetikler; View izniyle salt-okunur sonuc yuzeyini AppService'e yonlendirir.
/// <summary>Contract check tetikleme, gecmis ve rapor islemleri.</summary>
[Route(ApiContractCheckerRoutes.Checks)]
[ApiExplorerSettings(GroupName = ApiContractCheckerSwaggerConstants.ChecksGroupName)]
[Authorize(ApiContractCheckerPermissions.Checks.View)]
public class ContractCheckRunController
    : EntityReadControllerBase<
        IContractCheckRunAppService,
        ContractCheckRunDetailDto,
        ContractCheckRunHeaderDto,
        GetContractCheckRunsInput>
{
    /// <summary>Iki mevcut snapshot icin asenkron contract check baslatir.</summary>
    [HttpPost]
    [Authorize(ApiContractCheckerPermissions.Checks.Execute)]
    [ProducesResponseType(typeof(Result<ContractCheckRunStatusDto>), StatusCodes.Status202Accepted)]
    public async Task<ActionResult<Result<ContractCheckRunStatusDto>>> Execute(
        [FromBody] ExecuteContractCheckDto input)
    {
        var result = await AppService.ExecuteAsync(input);
        Result<ContractCheckRunStatusDto> response = result;
        return AcceptedAtAction(nameof(GetStatus), new { id = result.Id }, response);
    }

    /// <summary>Run durumunu findings govdesini cekmeden getirir.</summary>
    [HttpGet(ApiContractCheckerRoutes.CheckStatus)]
    public async Task<Result<ContractCheckRunStatusDto>> GetStatus(Guid id)
    {
        var result = await AppService.GetStatusAsync(id);
        return result;
    }

    /// <summary>Run findings govdesinden anlik deterministik rapor uretir.</summary>
    [HttpGet(ApiContractCheckerRoutes.CheckReport)]
    public async Task<Result<ContractCheckReportDto>> GetReport(Guid id)
    {
        var result = await AppService.GetReportAsync(id);
        return result;
    }

    /// <summary>Run bulgularini degisim durumu ve filtrelerle sayfali getirir.</summary>
    [HttpGet(ApiContractCheckerRoutes.CheckFindings)]
    public async Task<Result<FindingPagedResultDto>> GetFindings(
        Guid id,
        [FromQuery] GetContractCheckFindingsInput input)
    {
        var result = await AppService.GetFindingsAsync(id, input);
        return result;
    }
}
