using System;
using System.Threading;
using System.Threading.Tasks;
using Ptn.TestModule.Dtos.Bridge;
using Ptn.TestModule.Managers.Bridge;
using Ptn.TestModule.Mappers.Bridge;
using Volo.Abp;
using CheckerWriteSetCapabilityAppService = Ptn.DatabaseChecker.Services.Capabilities.IWriteSetCapabilityAppService;

namespace Ptn.TestModule.Services.Bridge;

// islevi: Database checker yazma kumesi yuzeyini Bridge capability sozlesmesine baglar.
// sistemdeki gorevi: Checker cagrisi, Manager karari ve Mapperly eslemesini duz Application akisinda siralar.
[RemoteService(IsEnabled = false)]
public class WriteSetCapabilityAppService : TestModuleAppService, IWriteSetCapabilityAppService
{
    private static readonly PtnBridgeMapper Mapper = new();
    private readonly CheckerWriteSetCapabilityAppService _appService;
    private readonly FootprintCapabilityManager _manager;

    // Checker public servisini capability karar sahibine baglar.
    public WriteSetCapabilityAppService(
        CheckerWriteSetCapabilityAppService appService,
        FootprintCapabilityManager manager)
    {
        _appService = appService;
        _manager = manager;
    }

    // Checker capability olgusunu Bridge sozlugune ve butce sinirina cevirir.
    public async Task<CapabilityLevelDto> ProbeCapabilityAsync(
        Guid connectionId,
        bool hasExclusiveSandbox,
        CancellationToken cancellationToken)
    {
        var result = await _appService.ProbeAsync(
            Mapper.Map(_manager.CreateProbeRequest(connectionId, hasExclusiveSandbox)),
            cancellationToken);
        return Mapper.Map(_manager.ResolveCapability(Mapper.Map(result)));
    }

    // Checker capture sonucunu advisory Bridge footprint sozlesmesine cevirir.
    public async Task<FootprintResultDto> CaptureWriteSetAsync(
        Guid connectionId,
        Guid captureId,
        CancellationToken cancellationToken)
    {
        var result = await _appService.CaptureAsync(
            Mapper.Map(_manager.CreateCaptureRequest(connectionId, captureId)),
            cancellationToken);
        return Mapper.Map(_manager.Normalize(Mapper.Map(result)));
    }

    // Crash-cleanup release istegini slot sahibi checker'a devreder.
    public async Task ReleaseAsync(
        Guid connectionId,
        Guid captureId,
        CancellationToken cancellationToken)
    {
        await _appService.ReleaseAsync(connectionId, captureId, cancellationToken);
    }
}
