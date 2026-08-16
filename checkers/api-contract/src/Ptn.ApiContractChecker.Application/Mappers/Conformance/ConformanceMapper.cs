using Ptn.ApiContractChecker.Dtos.Conformance;
using Ptn.ApiContractChecker.Models.Conformance;
using Riok.Mapperly.Abstractions;

namespace Ptn.ApiContractChecker.Application.Mappers.Conformance;

// islevi: Conformance HTTP DTO'lari ile deger tasimayan domain oracle modellerini iki yonde tasir.
// sistemdeki gorevi: AppService'teki tum katmanlar arasi alan tasimanin tek sahibidir.
[Mapper]
public partial class ConformanceMapper
{
    [MapperIgnoreSource(nameof(ResponseConformanceDto.SnapshotId))]
    public partial ResponseConformanceRequest MapToRequest(ResponseConformanceDto input);

    [MapperIgnoreSource(nameof(RequestConformanceDto.SnapshotId))]
    public partial RequestConformanceRequest MapToRequest(RequestConformanceDto input);

    [MapperIgnoreSource(nameof(OperationSelectionDto.SnapshotId))]
    public partial OperationSelectionRequest MapToSelection(OperationSelectionDto input);

    public partial ConformanceResultDto MapToDto(ResponseConformanceResult result);
    public partial RequestExampleDto MapToDto(RequestExampleResult result);
    public partial OperationBindingResultDto MapToDto(OperationBindingResult result);

    [MapperIgnoreSource(nameof(AssertionDerivabilityDto.SnapshotId))]
    public partial AssertionDerivabilityRequest MapToRequest(AssertionDerivabilityDto input);

    public partial AssertionDerivabilityResultDto MapToDto(AssertionDerivabilityResult result);

    [MapperIgnoreSource(nameof(SampleSetRequestDto.SnapshotId))]
    public partial SampleSetRequest MapToRequest(SampleSetRequestDto input);

    public partial SampleSetResultDto MapToDto(SampleSetResult result);

    [MapperIgnoreSource(nameof(OperationLinkRequestDto.SnapshotId))]
    public partial OperationLinkRequest MapToRequest(OperationLinkRequestDto input);

    public partial OperationLinkResultDto MapToDto(OperationLinkResult result);
}
