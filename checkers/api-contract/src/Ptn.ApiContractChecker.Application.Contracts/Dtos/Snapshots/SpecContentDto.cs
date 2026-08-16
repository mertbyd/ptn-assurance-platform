namespace Ptn.ApiContractChecker.Dtos.Snapshots;

// islevi: Bir snapshot'in isaret ettigi degismez spec iceriginin API cikti temsilidir.
// sistemdeki gorevi: Ham metni, olculerini ve hash kimliklerini snapshot detayinin ayri bir alani olarak tasir.
public class SpecContentDto
{
    // Ham baytlarin SHA-256 kimligi ve tenant icindeki dedup anahtari.
    public string RawHash { get; set; } = default!;

    // Bicim gurultusu elenmis kanonik metnin SHA-256 kimligi.
    public string CanonicalHash { get; set; } = default!;

    // Kaynaktan alinan ham spec metni.
    public string Content { get; set; } = default!;

    // Ham icerigin UTF-8 bayt boyutu.
    public int ByteSize { get; set; }

    // Ham icerigin HTTP medya tipi.
    public string MediaType { get; set; } = default!;
}
