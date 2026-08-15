using Ptn.TestModule.Dtos.Bridge;
using Ptn.TestModule.Dtos.Bridge.Api;
using Ptn.TestModule.Dtos.Bridge.Database;
using Ptn.TestModule.Models.Bridge;
using Ptn.TestModule.Models.Bridge.Agent;
using Ptn.TestModule.Models.Bridge.Database;
using Ptn.TestModule.Models.Bridge.Footprint;
using Riok.Mapperly.Abstractions;
using CheckerCapabilityLevelDto = Ptn.DatabaseChecker.Dtos.Capabilities.CapabilityLevelDto;
using CheckerCapabilityProbeRequestDto = Ptn.DatabaseChecker.Dtos.Capabilities.CapabilityProbeRequestDto;
using CheckerWriteSetCaptureRequestDto = Ptn.DatabaseChecker.Dtos.Capabilities.WriteSetCaptureRequestDto;
using CheckerWriteSetResultDto = Ptn.DatabaseChecker.Dtos.Capabilities.WriteSetResultDto;
using CheckerWriteSetTableDeltaDto = Ptn.DatabaseChecker.Dtos.Capabilities.WriteSetTableDeltaDto;

namespace Ptn.TestModule.Mappers.Bridge;

// islevi: Bridge agent request/response DTO'lariyla domain modellerini compile-time esler.
// sistemdeki gorevi: AppService ve capability servisindeki tum alan kopyalamanin tek bildirime dayali sahibidir.
[Mapper]
public partial class PtnBridgeMapper
{
    public partial GroundRequest Map(GroundRequestDto input);
    public partial ExplainRequest Map(ExplainRequestDto input);
    public partial ValidateRequest Map(ValidateRequestDto input);
    public partial KnowledgeRequest Map(KnowledgeRequestDto input);
    public partial CapabilityLevel Map(CapabilityLevelDto input);
    public partial CheckerCapabilityLevel Map(CheckerCapabilityLevelDto input);
    public partial CheckerCapabilityProbeRequestDto Map(CapabilityProbeRequest input);
    public partial CheckerWriteSetCaptureRequestDto Map(WriteSetCaptureRequest input);
    public partial FootprintResult Map(CheckerWriteSetResultDto input);
    public partial GroundResultDto Map(GroundingResult input);
    public partial ValidateResultDto Map(ValidationResult input);
    public partial KnowledgeResultDto Map(KnowledgeResult input);
    public partial ToolCatalogDto Map(ToolCatalog input);
    public partial AgentProfileDto Map(AgentProfile input);
    public partial ToolBudgetDecisionDto Map(ToolBudgetDecision input);
    public partial McpTaskStatusDto Map(McpTaskStatus input);
    public partial OverlayPatchSuggestionDto Map(OverlayPatchSuggestion input);
    public partial CapabilityLevelDto Map(CapabilityLevel input);
    public partial FootprintResultDto Map(FootprintResult input);
    public partial CoverageReportDto Map(CoverageReport input);
    public partial ClosedQuestionDto Map(ClosedQuestion input);
    public partial RowDeltaDto Map(RowDelta input);
    public partial RowDelta Map(CheckerWriteSetTableDeltaDto input);
    public partial OperationBindingDto Map(OperationBinding input);
    public partial OperationSuggestionDto Map(OperationSuggestion input);
    public partial FieldBindingDto Map(FieldBinding input);
    public partial RequestExampleDto Map(RequestExample input);
    public partial TableDescriptionDto Map(TableDescription input);
    public partial TableColumnDto Map(TableColumn input);
    public partial TableKeyDto Map(TableKey input);
    public partial DerivabilityResultDto Map(DerivabilityResult input);
    public partial DerivabilityItemDto Map(DerivabilityItem input);
    private partial DatabaseDerivabilityAddress Map(DatabaseDerivabilityAddressDto input);
    private partial DatabaseDerivabilityResultDto Map(DatabaseDerivabilityResult input);
    private partial DatabaseDerivabilityItemDto Map(DatabaseDerivabilityItem input);
}
