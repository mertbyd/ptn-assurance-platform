using Ptn.TestModule.Dtos.Bridge;
using Ptn.TestModule.Dtos.Bridge.Diagnosis;
using Ptn.TestModule.Models.Bridge;
using Ptn.TestModule.Models.Bridge.Agent;
using Riok.Mapperly.Abstractions;

namespace Ptn.TestModule.Mappers.Bridge;

// islevi: Domain aciklama agaci, kanit, konum ve finding modellerini public DTO agacina esler.
// sistemdeki gorevi: Recursive explanation donusumunu AppService govdesinden tamamen cikarir.
[Mapper]
public partial class PtnExplanationMapper
{
    public partial PtnExplainResultDto Map(PtnExplainResult input);
    public partial PtnExplanationNodeDto Map(PtnExplanationNode input);
    public partial PtnEvidenceDto Map(PtnEvidence input);
    public partial LocationDto Map(PtnLocation input);
    public partial FindingRefDto Map(PtnFindingRef input);
    public partial PtnCoverageReportDto Map(PtnCoverageReport input);
    public partial PtnClosedQuestionDto Map(PtnClosedQuestion input);
}
