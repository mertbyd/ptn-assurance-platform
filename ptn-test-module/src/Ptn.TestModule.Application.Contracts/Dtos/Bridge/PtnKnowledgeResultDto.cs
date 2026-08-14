namespace Ptn.TestModule.Dtos.Bridge;

// islevi: ptn_knowledge profil kapsami ve kapali kavram kodu sonucunu tasir.
// sistemdeki gorevi: Bilgiyi model anlatimi yerine surumlu profil kaynagindan gelen kodlarla sunar.
public sealed class PtnKnowledgeResultDto
{
    public string ResponseFormat { get; set; } = string.Empty;
    public PtnCoverageReportDto Coverage { get; set; } = new();
    public List<string> ConceptCodes { get; set; } = [];
    public string? ResourceLink { get; set; }
}
