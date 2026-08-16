namespace Ptn.TestModule.Dtos.Authoring;

// islevi: Yuklu profil paketinin anahtar, muhur ve kapsam sayilarini disari acar.
// sistemdeki gorevi: Kavram baglamasinin ne kadarinin onayli oldugunu paket icerigini sizdirmadan gosterir.
public sealed class ProfilePackSummaryDto
{
    /// <summary>
    /// Paketin dosya adini turettigi kararli anahtarini belirtir.
    /// </summary>
    public string ProfileKey { get; set; } = string.Empty;
    /// <summary>
    /// Paketin kendi bildirdigi revizyon etiketini belirtir.
    /// </summary>
    public string Revision { get; set; } = string.Empty;
    /// <summary>
    /// Paket icerigi muhrunu lowercase sha256: sozlesmesinde belirtir.
    /// </summary>
    public string ContentFingerprint { get; set; } = string.Empty;
    /// <summary>
    /// Paketin baglandigi veritabani sema muhrunu belirtir.
    /// </summary>
    public string DbSchemaFingerprint { get; set; } = string.Empty;
    /// <summary>
    /// Isleme ait sayisal sinir, sira veya durum degerini belirtir.
    /// </summary>
    public int BindingCount { get; set; }
    /// <summary>
    /// Isleme ait sayisal sinir, sira veya durum degerini belirtir.
    /// </summary>
    public int ApprovedBindingCount { get; set; }
    /// <summary>
    /// Isleme ait sayisal sinir, sira veya durum degerini belirtir.
    /// </summary>
    public int EvidencePathCount { get; set; }
}
