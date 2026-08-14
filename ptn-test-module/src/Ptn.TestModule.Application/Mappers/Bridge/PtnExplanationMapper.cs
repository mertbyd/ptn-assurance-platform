using Ptn.TestModule.Dtos.Bridge;
using Ptn.TestModule.Dtos.Bridge.Diagnosis;
using Ptn.TestModule.Models.Bridge;
using Ptn.TestModule.Models.Bridge.Agent;
using Riok.Mapperly.Abstractions;
using BridgeEvidenceDto = Ptn.TestModule.Dtos.Bridge.EvidenceDto;

namespace Ptn.TestModule.Mappers.Bridge;

// islevi: Domain aciklama agaci, kanit, konum ve finding modellerini public DTO agacina esler.
// sistemdeki gorevi: Recursive explanation donusumunu AppService govdesinden tamamen cikarir.
[Mapper]
public partial class PtnExplanationMapper
{
    public partial ExplainResultDto Map(ExplainResult input);
    public partial ExplanationNodeDto Map(ExplanationNode input);
    public partial BridgeEvidenceDto Map(Evidence input);
    public partial LocationDto Map(Location input);
    public partial FindingRefDto Map(FindingRef input);
    public partial CoverageReportDto Map(CoverageReport input);
    public partial ClosedQuestionDto Map(ClosedQuestion input);
}
