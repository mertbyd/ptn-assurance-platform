using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ptn.ApiContractChecker.Constants;
using Ptn.ApiContractChecker.Dtos.Snapshots;
using Ptn.ApiContractChecker.Permissions;
using Ptn.ApiContractChecker.Services.Snapshots;
using SystemStandards.Results;
using Volo.Abp.Application.Dtos;
using Ptn.ApiContractChecker.Dtos.Conformance;

namespace Ptn.ApiContractChecker.Controllers.Snapshots;

// islevi: Bir kaynak dokumaninin snapshot gecmisi ile tekil anlik goruntu endpointlerini HTTP uzerinden acar.
// sistemdeki gorevi: Gecmis listesini kaynak/dokuman rotasi altinda kapsarken detayi kararli snapshot kimligiyle acan ince koprudur.
/// <summary>Spec anlik goruntu gecmisi okuma islemleri.</summary>
[Route(ApiContractCheckerRoutes.Snapshots)]
[ApiExplorerSettings(GroupName = ApiContractCheckerSwaggerConstants.SourcesGroupName)]
[Authorize(ApiContractCheckerPermissions.Sources.View)]
public class SpecSnapshotController : ApiContractCheckerController
{
    // Snapshot okuma senaryolarini yurutun application contract'i.
    private ISpecSnapshotAppService AppService => LazyGetRequiredService<ISpecSnapshotAppService>();

    /// <summary>Tek kaynak dokumaninin snapshot gecmisini alinma zamani azalan sirada sayfali listeler.</summary>
    [HttpGet(ApiContractCheckerRoutes.SourceDocumentSnapshots)]
    public async Task<Result<PagedResultDto<SpecSnapshotHeaderDto>>> GetList(
        Guid id,
        Guid documentId,
        [FromQuery] GetSpecSnapshotsInput input)
    {
        input.SpecSourceId = id;
        input.SpecDocumentId = documentId;
        var result = await AppService.GetListAsync(input);
        return result;
    }

    /// <summary>Tek snapshot detayini bagli icerik ve format satirlariyla birlikte getirir.</summary>
    [HttpGet(ApiContractCheckerRoutes.EntityById)]
    public async Task<Result<SpecSnapshotDetailDto>> Get(Guid id)
    {
        var result = await AppService.GetAsync(id);
        return result;
    }

    /// <summary>Snapshot belgesindeki operasyon envanterini filtreli ve butceli sayfa olarak listeler.</summary>
    [HttpGet(ApiContractCheckerRoutes.SnapshotOperations)]
    public async Task<Result<SnapshotOperationInventoryDto>> ListOperations(
        Guid id,
        [FromQuery] ListSnapshotOperationsInput input)
    {
        var result = await AppService.ListOperationsAsync(id, input);
        return result;
    }

    /// <summary>Tek operasyonun butceli yazarlik ozetini getirir.</summary>
    [HttpPost(ApiContractCheckerRoutes.SnapshotOperationFind)]
    public async Task<Result<OperationSummaryDto>> FindOperation(Guid id, OperationSelectionDto input)
    {
        input.SnapshotId = id;
        var result = await AppService.FindOperationAsync(input);
        return result;
    }

    /// <summary>Tek component semasinin bir-seviye ozetini getirir.</summary>
    [HttpPost(ApiContractCheckerRoutes.SnapshotSchemaDescribe)]
    public async Task<Result<SchemaDescriptionDto>> DescribeSchema(Guid id, DescribeSchemaDto input)
    {
        input.SnapshotId = id;
        var result = await AppService.DescribeSchemaAsync(input);
        return result;
    }

    /// <summary>Kirpilmis yazarlik cevabinin tam ozetini resultRef ile getirir.</summary>
    [HttpGet(ApiContractCheckerRoutes.SnapshotAuthoringResult)]
    public async Task<Result<SnapshotAuthoringResultDto>> GetAuthoringResult(string resultRef)
    {
        var result = await AppService.GetAuthoringResultAsync(resultRef);
        return result;
    }
}
