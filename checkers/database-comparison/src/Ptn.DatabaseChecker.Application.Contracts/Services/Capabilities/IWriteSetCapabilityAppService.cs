using Ptn.DatabaseChecker.Dtos.Capabilities;
using Volo.Abp.Application.Services;

namespace Ptn.DatabaseChecker.Services.Capabilities;

// islevi: Yazma kumesi capability probe, capture ve release public use-case'lerini tanimlar.
// sistemdeki gorevi: HTTP katmanini concrete Application servisinden ve provider I/O'dan ayirir.
public interface IWriteSetCapabilityAppService : IApplicationService
{
    Task<CapabilityLevelDto> ProbeAsync(
        CapabilityProbeRequestDto input,
        CancellationToken cancellationToken = default);

    Task<WriteSetResultDto> CaptureAsync(
        WriteSetCaptureRequestDto input,
        CancellationToken cancellationToken = default);

    Task<WriteSetResultDto> ReleaseAsync(
        Guid connectionId,
        Guid captureRef,
        CancellationToken cancellationToken = default);
}
