using Ptn.TestModule.Dtos.Bridge;

namespace Ptn.TestModule.Dtos.Authoring;

// islevi: Grounding istegi ile mekanik Arazzo workflow kabugunun public baslangic girdisini tasir.
// sistemdeki gorevi: Yazarlik oturumunu gercek checker kanitina ve sourceDescription adreslerine baglar.
public sealed class CreateAuthoringSessionDto
{
    public GroundRequestDto Grounding { get; set; } = new();
    public string WorkflowId { get; set; } = string.Empty;
    public string WorkflowSummary { get; set; } = string.Empty;
    public string ApiSourceUrl { get; set; } = string.Empty;
    public string DatabaseSourceUrl { get; set; } = string.Empty;
}
