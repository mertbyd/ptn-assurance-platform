namespace Ptn.TestModule.Dtos.Bridge;

// islevi: ptn_knowledge profil kapsami ve kapali kavram kodu sonucunu tasir.
// sistemdeki gorevi: Bilgiyi model anlatimi yerine surumlu profil kaynagindan gelen kodlarla sunar.
public sealed class KnowledgeResultDto
{
    /// <summary>
    /// Cevabin concise veya ayrintili sunum bicimini belirtir.
    /// </summary>
    public string ResponseFormat { get; set; } = string.Empty;
    /// <summary>
    /// Profilin kavram kapsama ozetini tasir.
    /// </summary>
    public CoverageReportDto Coverage { get; set; } = new();
    /// <summary>
    /// Kontrollu sozlukteki ilgili kodlari kararli sirada listeler.
    /// </summary>
    public List<string> ConceptCodes { get; set; } = [];
    /// <summary>
    /// Ayrintili kaynaga erisim adresini belirtir.
    /// </summary>
    public string? ResourceLink { get; set; }
}
