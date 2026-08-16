namespace Ptn.TestModule.Models.Bridge;

// islevi: Yuklu profil paketinin anahtar, muhur ve kapsam sayilarini ozet olarak tasir.
// sistemdeki gorevi: Listeleme ucunun paketin tamamini disari acmadan kapsamini gostermesini saglar.
public sealed class ProfilePackSummary
{
    public string ProfileKey { get; set; } = string.Empty;
    public string Revision { get; set; } = string.Empty;
    public string ContentFingerprint { get; set; } = string.Empty;
    public string DbSchemaFingerprint { get; set; } = string.Empty;
    public int BindingCount { get; set; }
    public int ApprovedBindingCount { get; set; }
    public int EvidencePathCount { get; set; }
}
