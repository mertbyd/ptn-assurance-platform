using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ptn.ApiContractChecker.Constants;
using Ptn.ApiContractChecker.Dtos.Snapshots;
using Ptn.ApiContractChecker.Dtos.Sources;
using Ptn.ApiContractChecker.Permissions;
using Ptn.ApiContractChecker.Services.Sources;
using SystemStandards.Results;

namespace Ptn.ApiContractChecker.Controllers.Sources;

// islevi: SpecSource okuma, yazma, pasiflestirme ve erisilebilirlik endpointlerini HTTP uzerinden acar.
// sistemdeki gorevi: SpecDocument icin ayri rota acmadan tum istekleri aggregate root AppService'ine yonlendiren ince koprudur.
/// <summary>
/// Spec kaynagi yonetim islemleri.
/// </summary>
[Route(ApiContractCheckerRoutes.Sources)]
[ApiExplorerSettings(GroupName = ApiContractCheckerSwaggerConstants.SourcesGroupName)]
[Authorize(ApiContractCheckerPermissions.Sources.View)]
public class SpecSourceController : EntityReadControllerBase<ISpecSourceAppService, SpecSourceDto>
{
    /// <summary>Yeni spec kaynagini dokumanlariyla kurar ve varsa kimlik bilgisini Vault'a yazar.</summary>
    [HttpPost]
    [Authorize(ApiContractCheckerPermissions.Sources.Manage)]
    public async Task<Result<SpecSourceDto>> Create([FromBody] CreateSpecSourceDto input)
    {
        var result = await AppService.CreateAsync(input);
        return result;
    }

    /// <summary>Kaynak tanimini, dokumanlarini ve verilirse Vault kimlik bilgisini gunceller.</summary>
    [HttpPut(ApiContractCheckerRoutes.EntityById)]
    [Authorize(ApiContractCheckerPermissions.Sources.Manage)]
    public async Task<Result<SpecSourceDto>> Update(Guid id, [FromBody] UpdateSpecSourceDto input)
    {
        var result = await AppService.UpdateAsync(id, input);
        return result;
    }

    /// <summary>Kaynagi fiziksel silmeden yeni kontroller icin pasiflestirir.</summary>
    [HttpPost(ApiContractCheckerRoutes.SourcePassivate)]
    [Authorize(ApiContractCheckerPermissions.Sources.Manage)]
    public async Task<Result<SpecSourceDto>> Passivate(Guid id)
    {
        var result = await AppService.PassivateAsync(id);
        return result;
    }

    /// <summary>Aktif kaynak dokumanlarinin HTTP erisilebilirligini credential sizdirmadan test eder.</summary>
    [HttpPost(ApiContractCheckerRoutes.SourceReachabilityTest)]
    [Authorize(ApiContractCheckerPermissions.Sources.Manage)]
    public async Task<Result<SpecSourceReachabilityDto>> Test(Guid id)
    {
        var result = await AppService.TestReachabilityAsync(id);
        return result;
    }

    /// <summary>Tek aktif dokumanin zamanlanmis izlemesini acar veya kapatir.</summary>
    [HttpPost(ApiContractCheckerRoutes.SourceDocumentMonitoring)]
    [Authorize(ApiContractCheckerPermissions.Sources.Manage)]
    public async Task<Result<SpecDocumentMonitoringDto>> ConfigureMonitoring(
        Guid id,
        Guid documentId,
        [FromBody] ConfigureSpecDocumentMonitoringDto input)
    {
        var result = await AppService.ConfigureDocumentMonitoringAsync(id, documentId, input);
        return result;
    }

    /// <summary>Tek aktif dokumanin canli spec'ini cekip snapshot alir.</summary>
    [HttpPost(ApiContractCheckerRoutes.SourceDocumentSnapshot)]
    [Authorize(ApiContractCheckerPermissions.Sources.Manage)]
    public async Task<Result<SpecSnapshotDto>> CaptureSnapshot(Guid id, Guid documentId)
    {
        var result = await AppService.CaptureSnapshotAsync(id, documentId);
        return result;
    }
}
