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
using CheckerOperationQueryDto = Ptn.ApiContractChecker.Dtos.Conformance.OperationSelectionDto;
using CheckerOperationSuggestionDto = Ptn.ApiContractChecker.Dtos.Conformance.OperationBindingSuggestionDto;
using CheckerRequestExampleDto = Ptn.ApiContractChecker.Dtos.Conformance.RequestExampleDto;
using CheckerResponseObservationDto = Ptn.ApiContractChecker.Dtos.Conformance.ResponseConformanceDto;

namespace Ptn.TestModule.Mappers.Bridge;

// islevi: API checker DTO'lariyla Bridge modelleri arasindaki compile-time eslemeleri uretir.
// sistemdeki gorevi: Application servisini property kopyalamadan checker cagrisi ve normalizasyona indirger.
[Mapper]
public partial class ApiOracleMapper
{
    public partial PtnOperationQuery Map(OperationQueryDto input);
    public partial OperationBindingDto Map(PtnOperationBinding input);
    public partial RequestExampleDto Map(PtnRequestExample input);
    public partial PtnDerivabilityRequest Map(DerivabilityRequestDto input);
    public partial DerivabilityResultDto Map(PtnDerivabilityResult input);
    public partial PtnResponseObservation Map(ResponseObservationDto input);
    public partial ConformanceResultDto Map(PtnConformanceResult input);
    public partial CheckerOperationQueryDto Map(PtnApiOperationRequest input);
    public partial CheckerDerivabilityRequestDto Map(PtnDerivabilityRequest input);
    public partial CheckerResponseObservationDto Map(PtnApiResponseRequest input);
    public partial PtnOperationBinding Map(CheckerOperationBindingDto input);
    public partial PtnRequestExample Map(CheckerRequestExampleDto input);
    public partial PtnDerivabilityResult Map(CheckerDerivabilityResultDto input);
    public partial PtnConformanceResult Map(CheckerConformanceResultDto input);
    public partial PtnOperationSuggestion MapSuggestion(CheckerOperationSuggestionDto input);
    public partial PtnFieldBinding MapFieldBinding(CheckerFieldBindingDto input);
    public partial PtnDerivabilityItem MapDerivabilityItem(CheckerDerivabilityItemDto input);
    public partial PtnConformanceViolation MapViolation(CheckerConformanceViolationDto input);
}
