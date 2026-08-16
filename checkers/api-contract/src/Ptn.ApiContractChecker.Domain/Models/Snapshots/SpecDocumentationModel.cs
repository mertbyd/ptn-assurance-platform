namespace Ptn.ApiContractChecker.Models.Snapshots;

// islevi: Summary, description ve example metinlerini yapisal sozlesmeden ayri tasir.
// sistemdeki gorevi: Metin degisikliklerinin kaybolmadan DocsOnly olarak ele alinmasini ve yapisal farka karismamasini saglar.
public class SpecDocumentationModel
{
    // Dokumantasyonun bagli oldugu nesne turu.
    public string TargetKind { get; set; } = string.Empty;

    // Dokumantasyonun bagli oldugu nesnenin kararli adresi.
    public string Target { get; set; } = string.Empty;

    // Kisa ozet metni.
    public string? Summary { get; set; }

    // Ayrintili aciklama metni.
    public string? Description { get; set; }

    // Ornek degerin kararli metin gosterimi.
    public string? Example { get; set; }

    // Bu modeldeki degisikliklerin davranissal siniflandirmaya girmedigini bildirir.
    public bool IsDocumentationOnly => true;
}
