using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ptn.DatabaseChecker.Constants;
using Ptn.DatabaseChecker.Dtos.Capabilities;
using Ptn.DatabaseChecker.Permissions;
using Ptn.DatabaseChecker.Services.Capabilities;
using SystemStandards.Results;

namespace Ptn.DatabaseChecker.Controllers.Capabilities;

// islevi: Yazma kumesi capability probe, capture ve release endpointlerini HTTP uzerinden acar.
// sistemdeki gorevi: Named permission ve binding metadata'si disinda karar tasimayan ince transport wrapper'idir.
/// <summary>
/// Advisory yazma kumesi capability islemleri.
/// </summary>
[Route(DatabaseCheckerHttpApiConstants.Routes.WriteSetCapabilities)]
[ApiExplorerSettings(GroupName = DatabaseCheckerHttpApiConstants.Groups.Capabilities)]
public class WriteSetCapabilityController : DatabaseCheckerController
{
    private IWriteSetCapabilityAppService AppService
        => LazyServiceProvider.LazyGetRequiredService<IWriteSetCapabilityAppService>();

    /// <summary>
    /// Kayitli hedefin logical decoding ve diff fallback yetenegini baglayici sirayla yoklar.
    /// </summary>
    [HttpPost(DatabaseCheckerHttpApiConstants.Segments.WriteSetProbe)]
    [Authorize(DatabaseCheckerPermissions.Capabilities.Probe)]
    public async Task<Result<CapabilityLevelDto>> Probe(
        [FromBody] CapabilityProbeRequestDto input,
        CancellationToken cancellationToken)
    {
        var result = await AppService.ProbeAsync(input, cancellationToken);
        return result;
    }

    /// <summary>
    /// Temporary slot veya once/sonra farki ile advisory yazma kumesi yakalar.
    /// </summary>
    [HttpPost(DatabaseCheckerHttpApiConstants.Segments.WriteSetCapture)]
    [Authorize(DatabaseCheckerPermissions.Capabilities.Capture)]
    public async Task<Result<WriteSetResultDto>> Capture(
        [FromBody] WriteSetCaptureRequestDto input,
        CancellationToken cancellationToken)
    {
        var result = await AppService.CaptureAsync(input, cancellationToken);
        return result;
    }

    /// <summary>
    /// Cokme sonrasi kalmis capture kaynagini idempotent olarak serbest birakir.
    /// </summary>
    [HttpPost(DatabaseCheckerHttpApiConstants.Segments.WriteSetRelease)]
    [Authorize(DatabaseCheckerPermissions.Capabilities.Capture)]
    public async Task<Result<WriteSetResultDto>> Release(
        [FromQuery] Guid connectionId,
        [FromQuery] Guid captureRef,
        CancellationToken cancellationToken)
    {
        var result = await AppService.ReleaseAsync(connectionId, captureRef, cancellationToken);
        return result;
    }
}
