namespace Ptn.TestModule.Dtos.Bridge;

// islevi: ptn_knowledge tool'unun profil ve kapali kavram kodu secimini tasir.
// sistemdeki gorevi: Bilgi sorgusunu serbest soru metni yerine profil kaynakli sozluk alanina indirger.
public sealed class PtnKnowledgeRequestDto
{
    public string ProfileKey { get; set; } = string.Empty;
    public Guid ConnectionId { get; set; }
    public List<string> ConceptCodes { get; set; } = [];
    public string ResponseFormat { get; set; } = string.Empty;
}
