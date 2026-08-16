using Ptn.ApiContractChecker.Entities.Lookups;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Ptn.ApiContractChecker.Entities.Snapshots;

// islevi: Bir spec dokumaninin belirli anda gordugu icerik ve format baglantisini kaydeder.
// sistemdeki gorevi: Contract check run'larinin karsilastirdigi ince zaman-serisi referansidir.
public class SpecSnapshot : CreationAuditedAggregateRoot<Guid>, IMultiTenant
{
    // Snapshot'in ait oldugu SpecDocument kimligi.
    public Guid SpecDocumentId { get; internal set; }

    // Snapshot'in isaret ettigi degismez SpecContent kimligi.
    public Guid SpecContentId { get; internal set; }

    // Okuyucu secimini belirleyen global SpecFormat lookup kimligi.
    public Guid SpecFormatId { get; internal set; }

    // Spec info.version degeri; dokumanda bulunmayabilir.
    public string? ApiVersion { get; internal set; }

    // Ayni ham icerigin en son goruldugu zaman.
    public DateTime LastSeenAt { get; internal set; }

    // Snapshot'in ABP tenant sahipligi.
    public Guid? TenantId { get; internal set; }

    // Snapshot'in isaret ettigi degismez icerik navigation'i.
    public SpecContent SpecContent { get; internal set; } = default!;

    // Snapshot'in bagli oldugu global format lookup navigation'i.
    public SpecFormat SpecFormat { get; internal set; } = default!;

    // EF Core materializasyonu icin parametresiz ctor.
    protected SpecSnapshot()
    {
    }

    // Snapshot alanlarini davranis uygulamadan atar; FK ve zaman kurallari SpecIngestionManager'a aittir.
    public SpecSnapshot(
        Guid id,
        Guid specDocumentId,
        Guid specContentId,
        Guid specFormatId,
        string? apiVersion,
        DateTime lastSeenAt,
        Guid? tenantId)
        : base(id)
    {
        SpecDocumentId = specDocumentId;
        SpecContentId = specContentId;
        SpecFormatId = specFormatId;
        ApiVersion = apiVersion;
        LastSeenAt = lastSeenAt;
        TenantId = tenantId;
    }
}
