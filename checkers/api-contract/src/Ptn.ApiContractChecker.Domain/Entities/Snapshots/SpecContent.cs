using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Ptn.ApiContractChecker.Entities.Snapshots;

// islevi: Ham spec metnini tenant icinde hash ile adreslenen degismez icerik olarak saklar.
// sistemdeki gorevi: Birden cok snapshot'in ayni icerigi tekrar yazmadan paylasabildigi ince veri tasiyicisidir.
public class SpecContent : CreationAuditedEntity<Guid>, IMultiTenant
{
    // Ham baytlarin SHA-256 hex kimligi ve tenant icindeki dedup anahtari.
    public string RawHash { get; internal set; } = default!;

    // Normalize edilmis modelin SHA-256 hex kimligi ve anlamsal esitlik anahtari.
    public string CanonicalHash { get; internal set; } = default!;

    // Kaynaktan alinan ham spec metni.
    public string Content { get; internal set; } = default!;

    // Ham icerigin UTF-8 bayt boyutu.
    public int ByteSize { get; internal set; }

    // Ham icerigin HTTP medya tipi.
    public string MediaType { get; internal set; } = default!;

    // Icerik dedup ve gorunurluk sinirinin tenant kimligi.
    public Guid? TenantId { get; internal set; }

    // EF Core materializasyonu icin parametresiz ctor.
    protected SpecContent()
    {
    }

    // Degismez icerik alanlarini davranis uygulamadan atar; dogrulama SpecIngestionManager'a aittir.
    public SpecContent(
        Guid id,
        string rawHash,
        string canonicalHash,
        string content,
        int byteSize,
        string mediaType,
        Guid? tenantId)
        : base(id)
    {
        RawHash = rawHash;
        CanonicalHash = canonicalHash;
        Content = content;
        ByteSize = byteSize;
        MediaType = mediaType;
        TenantId = tenantId;
    }
}
