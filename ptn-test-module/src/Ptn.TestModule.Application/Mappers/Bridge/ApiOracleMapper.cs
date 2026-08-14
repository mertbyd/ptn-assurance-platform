using Ptn.ApiContractChecker.Dtos.Conformance;
using Ptn.TestModule.Models.Bridge;
using Riok.Mapperly.Abstractions;

namespace Ptn.TestModule.Mappers.Bridge;

// islevi: API checker DTO'lariyla Bridge modelleri arasindaki compile-time eslemeleri uretir.
// sistemdeki gorevi: Application servisini property kopyalamadan checker cagrisi ve normalizasyona indirger.
[Mapper]
public partial class ApiOracleMapper
{
    public partial OperationSelectionDto Map(PtnOperationQuery input);
    public partial AssertionDerivabilityDto Map(PtnDerivabilityRequest input);
    public partial ResponseConformanceDto Map(PtnResponseObservation input);
    public partial PtnOperationBinding Map(OperationBindingResultDto input);
    public partial PtnRequestExample Map(RequestExampleDto input);
    public partial PtnDerivabilityResult Map(AssertionDerivabilityResultDto input);
    public partial PtnConformanceResult Map(ConformanceResultDto input);
    private partial PtnOperationSuggestion MapSuggestion(OperationBindingSuggestionDto input);
    private partial PtnFieldBinding MapFieldBinding(OperationFieldBindingDto input);
    private partial PtnDerivabilityItem MapDerivabilityItem(AssertionDerivabilityItemDto input);
    private partial PtnConformanceViolation MapViolation(ConformanceViolationDto input);
}
