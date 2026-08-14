using Ptn.TestModule.Dtos.Bridge.Api;
using Ptn.TestModule.Dtos.Bridge.Database;

namespace Ptn.TestModule.Dtos.Bridge;

// islevi: ptn_ground kararini, kritik olguyu ve birlesik agir govdeyi tek cevapta tasir.
// sistemdeki gorevi: Kapsam ve karari response basinda, ayrintiyi inline veya ResourceLink arkasinda sunar.
public sealed class PtnGroundResultDto
{
    public string ResponseFormat { get; set; } = string.Empty;
    public PtnCoverageReportDto Coverage { get; set; } = new();
    public string DecisionCode { get; set; } = string.Empty;
    public string CriticalFactCode { get; set; } = string.Empty;
    public OperationBindingDto? OperationBinding { get; set; }
    public RequestExampleDto? RequestExample { get; set; }
    public TableDescriptionDto? TableDescription { get; set; }
    public PtnFootprintResultDto Footprint { get; set; } = new();
    public List<PtnClosedQuestionDto> Questions { get; set; } = [];
    public string? ResourceLink { get; set; }
}
