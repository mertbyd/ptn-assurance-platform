namespace Ptn.TestModule.Dtos.Bridge;

// islevi: ptn_knowledge tool'unun profil ve kapali kavram kodu secimini tasir.
// sistemdeki gorevi: Bilgi sorgusunu serbest soru metni yerine profil kaynakli sozluk alanina indirger.
public sealed class KnowledgeRequestDto
{
    /// <summary>
    /// Kullanilacak profil paketinin kararli anahtarini belirtir.
    /// </summary>
    public string ProfileKey { get; set; } = string.Empty;
    /// <summary>
    /// Checker isleminin calisacagi baglantinin kimligini belirtir.
    /// </summary>
    public Guid ConnectionId { get; set; }
    /// <summary>
    /// Kontrollu sozlukteki ilgili kodlari kararli sirada listeler.
    /// </summary>
    public List<string> ConceptCodes { get; set; } = [];
    /// <summary>
    /// Cevabin concise veya ayrintili sunum bicimini belirtir.
    /// </summary>
    public string ResponseFormat { get; set; } = string.Empty;
}
