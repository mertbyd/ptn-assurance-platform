using Ptn.TestModule.Dtos.Bridge;
using Ptn.TestModule.Dtos.Bridge.Api;
using Ptn.TestModule.Dtos.Bridge.Database;
using Ptn.TestModule.Models.Bridge;
using Ptn.TestModule.Models.Bridge.Agent;
using Ptn.TestModule.Models.Bridge.Footprint;
using Riok.Mapperly.Abstractions;

namespace Ptn.TestModule.Mappers.Bridge;

// islevi: Bridge agent request/response DTO'lariyla domain modellerini compile-time esler.
// sistemdeki gorevi: AppService ve capability servisindeki tum alan kopyalamanin tek bildirime dayali sahibidir.
[Mapper]
public partial class PtnBridgeMapper
{
    public partial PtnGroundRequest Map(PtnGroundRequestDto input);
    public partial PtnExplainRequest Map(PtnExplainRequestDto input);
    public partial PtnValidateRequest Map(PtnValidateRequestDto input);
    public partial PtnKnowledgeRequest Map(PtnKnowledgeRequestDto input);
    public partial PtnCapabilityLevel Map(PtnCapabilityLevelDto input);
    public partial PtnGroundResultDto Map(PtnGroundingResult input);
    public partial PtnValidateResultDto Map(PtnValidationResult input);
    public partial PtnKnowledgeResultDto Map(PtnKnowledgeResult input);
    public partial PtnToolCatalogDto Map(PtnToolCatalog input);
    public partial PtnCapabilityLevelDto Map(PtnCapabilityLevel input);
    public partial PtnFootprintResultDto Map(PtnFootprintResult input);
    public partial PtnCoverageReportDto Map(PtnCoverageReport input);
    public partial PtnClosedQuestionDto Map(PtnClosedQuestion input);
    public partial PtnRowDeltaDto Map(PtnRowDelta input);
    public partial OperationBindingDto Map(PtnOperationBinding input);
    public partial OperationSuggestionDto Map(PtnOperationSuggestion input);
    public partial FieldBindingDto Map(PtnFieldBinding input);
    public partial RequestExampleDto Map(PtnRequestExample input);
    public partial TableDescriptionDto Map(PtnTableDescription input);
    public partial TableColumnDto Map(PtnTableColumn input);
    public partial TableKeyDto Map(PtnTableKey input);
    public partial DerivabilityResultDto Map(PtnDerivabilityResult input);
    public partial DerivabilityItemDto Map(PtnDerivabilityItem input);
}
