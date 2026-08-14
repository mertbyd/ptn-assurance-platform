using System.Collections.Generic;
using Ptn.TestModule.Models.Bridge;

namespace Ptn.TestModule.Models.Bridge.Agent;

// islevi: Profil kavram kapsamini response bicimi ve agir govde baglantisiyla tasir.
// sistemdeki gorevi: ptn_knowledge cevabini serbest anlatim yerine profil kaynakli kapali kodlara baglar.
public sealed class PtnKnowledgeResult
{
    public string ResponseFormat { get; set; } = string.Empty;
    public PtnCoverageReport Coverage { get; set; } = new();
    public List<string> ConceptCodes { get; set; } = [];
    public string? ResourceLink { get; set; }
}
