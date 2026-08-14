using Ptn.TestModule.Dtos.Bridge.Diagnosis;

namespace Ptn.TestModule.Dtos.Bridge;

// islevi: Yurutulen kanit adiminin durum, alaka, konum, kanit ve alt dugumlerini tasir.
// sistemdeki gorevi: Aciklamayi ikinci akil yurutme yerine mekanik yurutme izi olarak sunar.
public sealed class PtnExplanationNodeDto
{
    public string NodeKindCode { get; set; } = string.Empty;
    public string StateCode { get; set; } = string.Empty;
    public string RelevanceCode { get; set; } = string.Empty;
    public LocationDto Location { get; set; } = new();
    public List<PtnEvidenceDto> Evidence { get; set; } = [];
    public List<PtnExplanationNodeDto> Children { get; set; } = [];
}
