using Ptn.TestModule.Dtos.Bridge;
using Ptn.TestModule.Dtos.Bridge.Api;
using Ptn.TestModule.Models.Bridge;
using Ptn.TestModule.Models.Bridge.Api;
using Riok.Mapperly.Abstractions;
using CheckerConformanceResultDto = Ptn.ApiContractChecker.Dtos.Conformance.ConformanceResultDto;
using CheckerConformanceViolationDto = Ptn.ApiContractChecker.Dtos.Conformance.ConformanceViolationDto;
using CheckerDerivabilityItemDto = Ptn.ApiContractChecker.Dtos.Conformance.AssertionDerivabilityItemDto;
using CheckerDerivabilityRequestDto = Ptn.ApiContractChecker.Dtos.Conformance.AssertionDerivabilityDto;
using CheckerDerivabilityResultDto = Ptn.ApiContractChecker.Dtos.Conformance.AssertionDerivabilityResultDto;
using CheckerFieldBindingDto = Ptn.ApiContractChecker.Dtos.Conformance.OperationFieldBindingDto;
using CheckerOperationBindingDto = Ptn.ApiContractChecker.Dtos.Conformance.OperationBindingResultDto;
using CheckerOperationLinkCandidateDto = Ptn.ApiContractChecker.Dtos.Conformance.OperationLinkCandidateDto;
using CheckerOperationLinkParameterBindingDto = Ptn.ApiContractChecker.Dtos.Conformance.OperationLinkParameterBindingDto;
using CheckerOperationLinkRequestDto = Ptn.ApiContractChecker.Dtos.Conformance.OperationLinkRequestDto;
using CheckerOperationLinkResultDto = Ptn.ApiContractChecker.Dtos.Conformance.OperationLinkResultDto;
using CheckerOperationQueryDto = Ptn.ApiContractChecker.Dtos.Conformance.OperationSelectionDto;
using CheckerOperationSuggestionDto = Ptn.ApiContractChecker.Dtos.Conformance.OperationBindingSuggestionDto;
using CheckerRequestExampleDto = Ptn.ApiContractChecker.Dtos.Conformance.RequestExampleDto;
using CheckerResponseObservationDto = Ptn.ApiContractChecker.Dtos.Conformance.ResponseConformanceDto;
using CheckerCorrelationRefDto = Ptn.ApiContractChecker.Dtos.Correlation.CorrelationRefDto;

namespace Ptn.TestModule.Mappers.Bridge;

// islevi: API checker DTO'lariyla Bridge modelleri arasindaki compile-time eslemeleri uretir.
// sistemdeki gorevi: Application servisini property kopyalamadan checker cagrisi ve normalizasyona indirger.
[Mapper]
public partial class ApiOracleMapper
{
    public partial OperationQuery Map(OperationQueryDto input);
    public partial OperationBindingDto Map(OperationBinding input);
    public partial OperationLinkRequest Map(OperationLinkRequestDto input);
    public partial OperationLinkResultDto Map(OperationLinkResult input);
    public partial RequestExampleDto Map(RequestExample input);
    public partial DerivabilityRequest Map(DerivabilityRequestDto input);
    public partial DerivabilityRequestDto MapToDto(DerivabilityRequest input);
    public partial DerivabilityResultDto Map(DerivabilityResult input);
    public partial DerivabilityResult MapResult(DerivabilityResultDto input);
    public partial ResponseObservation Map(ResponseObservationDto input);
    public partial ResponseObservationDto MapToDto(ResponseObservation input);
    public partial ConformanceResultDto Map(ConformanceResult input);
    public partial ConformanceResult MapResult(ConformanceResultDto input);
    public partial CheckerOperationQueryDto Map(ApiOperationRequest input);
    public partial CheckerOperationLinkRequestDto Map(OperationLinkRequest input);
    public partial CheckerDerivabilityRequestDto Map(DerivabilityRequest input);
    public partial CheckerResponseObservationDto Map(ApiResponseRequest input);
    public partial OperationBinding Map(CheckerOperationBindingDto input);
    public partial OperationLinkResult Map(CheckerOperationLinkResultDto input);
    public partial OperationLinkCandidate MapOperationLinkCandidate(CheckerOperationLinkCandidateDto input);
    public partial OperationLinkParameterBinding MapOperationLinkParameterBinding(CheckerOperationLinkParameterBindingDto input);
    public partial RequestExample Map(CheckerRequestExampleDto input);
    public partial DerivabilityResult Map(CheckerDerivabilityResultDto input);
    public partial ConformanceResult Map(CheckerConformanceResultDto input);
    public partial OperationSuggestion MapSuggestion(CheckerOperationSuggestionDto input);
    public partial FieldBinding MapFieldBinding(CheckerFieldBindingDto input);
    public partial DerivabilityItem MapDerivabilityItem(CheckerDerivabilityItemDto input);
    public partial ConformanceViolation MapViolation(CheckerConformanceViolationDto input);
    private partial CorrelationRef Map(CheckerCorrelationRefDto input);
    private partial CheckerCorrelationRefDto Map(CorrelationRef input);
    private partial CorrelationRef Map(CorrelationRefDto input);
    private partial CorrelationRefDto MapToDto(CorrelationRef input);
}
