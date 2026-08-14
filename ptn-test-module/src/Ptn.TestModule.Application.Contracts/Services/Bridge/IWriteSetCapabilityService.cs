using Ptn.TestModule.Dtos.Bridge;
using Volo.Abp.Application.Services;

namespace Ptn.TestModule.Services.Bridge;

// islevi: Yazma kumesi capability yoklama, yakalama ve birakma entegrasyon kontratini tanimlar.
// sistemdeki gorevi: Domain port uygulamasini public DTO'larla composition hostuna sunar.
public interface IWriteSetCapabilityService : IApplicationService
{
    Task<PtnCapabilityLevelDto> ProbeCapabilityAsync(
        Guid connectionId,
        bool hasExclusiveSandbox,
        CancellationToken cancellationToken);

    Task<PtnFootprintResultDto> CaptureWriteSetAsync(
        Guid connectionId,
        Guid captureId,
        CancellationToken cancellationToken);

    Task ReleaseAsync(Guid connectionId, Guid captureId, CancellationToken cancellationToken);
}
