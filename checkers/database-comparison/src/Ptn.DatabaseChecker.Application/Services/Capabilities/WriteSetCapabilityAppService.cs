using Ptn.DatabaseChecker.Application.Mappers.Capabilities;
using Ptn.DatabaseChecker.Dtos.Capabilities;
using Ptn.DatabaseChecker.Interface.Connections;
using Ptn.DatabaseChecker.Managers.Capabilities;
using Ptn.DatabaseChecker.Services.Capabilities;
using Volo.Abp;
using Volo.Abp.Uow;

namespace Ptn.DatabaseChecker.Application.Services.Capabilities;

// islevi: Kayitli baglanti, Mapperly ve WriteSetCapabilityManager arasinda probe/capture/release orkestrasyonu yapar.
// sistemdeki gorevi: Uzun hedef DB gozleminde UOW tutmayan ince Application siniridir; karar ve slot yonetmez.
[RemoteService(IsEnabled = false)]
[UnitOfWork(IsDisabled = true)]
public class WriteSetCapabilityAppService : DatabaseCheckerAppService, IWriteSetCapabilityAppService
{
    private static readonly WriteSetMapper Mapper = new();

    private WriteSetCapabilityManager Manager
        => LazyServiceProvider.LazyGetRequiredService<WriteSetCapabilityManager>();

    private IDatabaseConnectionRepository ConnectionRepository
        => LazyServiceProvider.LazyGetRequiredService<IDatabaseConnectionRepository>();

    // islevi: Gorulebilir baglantiyi bulur ve probe kararini Manager'a delege eder.
    public async Task<CapabilityLevelDto> ProbeAsync(
        CapabilityProbeRequestDto input,
        CancellationToken cancellationToken = default)
    {
        var connection = await ConnectionRepository.FindWithDetailsAsync(
            input.ConnectionId, cancellationToken);
        var result = await Manager.ProbeAsync(
            connection, Mapper.MapToRequest(input), cancellationToken);
        return Mapper.MapToLevelDto(result);
    }

    // islevi: Gorulebilir baglantiyi bulur ve exact/inferred capture stratejisini Manager'a delege eder.
    public async Task<WriteSetResultDto> CaptureAsync(
        WriteSetCaptureRequestDto input,
        CancellationToken cancellationToken = default)
    {
        var connection = await ConnectionRepository.FindWithDetailsAsync(
            input.ConnectionId, cancellationToken);
        var result = await Manager.CaptureAsync(
            connection, Mapper.MapToRequest(input), cancellationToken);
        return Mapper.MapToResultDto(result);
    }

    // islevi: Gorulebilir baglanti icin crash-cleanup release istegini Manager'a delege eder.
    public async Task<WriteSetResultDto> ReleaseAsync(
        Guid connectionId,
        Guid captureRef,
        CancellationToken cancellationToken = default)
    {
        var connection = await ConnectionRepository.FindWithDetailsAsync(
            connectionId, cancellationToken);
        var result = await Manager.ReleaseAsync(connection, captureRef, cancellationToken);
        return Mapper.MapToResultDto(result);
    }
}
