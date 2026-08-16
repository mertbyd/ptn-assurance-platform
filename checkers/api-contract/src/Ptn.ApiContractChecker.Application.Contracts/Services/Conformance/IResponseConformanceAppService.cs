using Ptn.ApiContractChecker.Dtos.Conformance;

namespace Ptn.ApiContractChecker.Services.Conformance;

// islevi: Response/request assertion ile request ornegi ve operasyon baglama yazarlik yuzeyini tanimlar.
public interface IResponseConformanceAppService
{
    Task<ConformanceResultDto> AssertResponseAsync(ResponseConformanceDto input);
    Task<ConformanceResultDto> AssertRequestAsync(RequestConformanceDto input);
    Task<RequestExampleDto> BuildRequestExampleAsync(OperationSelectionDto input);
    Task<OperationBindingResultDto> SuggestOperationBindingsAsync(OperationSelectionDto input);
    Task<AssertionDerivabilityResultDto> ValidateScenarioAssertionsAsync(AssertionDerivabilityDto input);
    Task<SampleSetResultDto> BuildSampleSetAsync(SampleSetRequestDto input);
    Task<OperationLinkResultDto> SuggestOperationLinksAsync(OperationLinkRequestDto input);
}
