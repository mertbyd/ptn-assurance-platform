using Ptn.TestModule.Dtos.Bridge.Api;
using Volo.Abp.Application.Services;

namespace Ptn.TestModule.Services.Bridge;

// islevi: API operasyon yazarligi ve response uygunluk kullanim senaryolarini tanimlar.
// sistemdeki gorevi: Bridge servis yuzeyini checker ve Domain modellerinden bagimsiz sunar.
public interface IApiOracleAppService : IApplicationService
{
    // Operasyon baglama adaylarini normalize public sonuc olarak getirir.
    Task<OperationBindingDto> SuggestOperationBindingsAsync(OperationQueryDto input, CancellationToken cancellationToken);

    // Secili kaynak operasyon icin kanitli sonraki adim adaylarini getirir.
    Task<OperationLinkResultDto> SuggestOperationLinksAsync(OperationLinkRequestDto input, CancellationToken cancellationToken);

    // Secili operasyon icin placeholder-isaretli request ornegi uretir.
    Task<RequestExampleDto> BuildRequestExampleAsync(OperationQueryDto input, CancellationToken cancellationToken);

    // Assertion yollarinin sozlesmeden turetilebilirligini denetler.
    Task<DerivabilityResultDto> ValidateScenarioAssertionsAsync(DerivabilityRequestDto input, CancellationToken cancellationToken);

    // Gozlenen HTTP yanitini API sozlesmesine karsi denetler.
    Task<ConformanceResultDto> AssertResponseAsync(ResponseObservationDto input, CancellationToken cancellationToken);
}
