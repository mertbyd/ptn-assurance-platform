using Ptn.ApiContractChecker.Models.Runs;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Ptn.ApiContractChecker.Entities.Runs;

// islevi: Iki spec snapshot'i arasindaki asenkron kontrolun alanlarini veri kabugu olarak saklar.
// sistemdeki gorevi: Durum gecisi, event ve sayac kararlarini manager'a birakip EF kaliciligi icin aggregate kimligini tasir.
public class ContractCheckRun : AuditedAggregateRoot<Guid>, IMultiTenant
{
    // Karsilastirmanin referans snapshot kimligi.
    public Guid BaseSnapshotId { get; internal set; }

    // Karsilastirmanin aday snapshot kimligi.
    public Guid TargetSnapshotId { get; internal set; }

    // Global CheckRunStatus lookup kimligi.
    public Guid CheckRunStatusId { get; internal set; }

    // Kuyruktan alip calistirmanin basladigi zaman.
    public DateTime? StartedAt { get; internal set; }

    // Terminal sonucun atomik yazildigi zaman.
    public DateTime? CompletedAt { get; internal set; }

    // Failed veya Partial terminal sonucunun kullaniciya guvenli sebebi.
    public string? ErrorMessage { get; internal set; }

    // Owned bulgulardan terminal geciste hesaplanan kirici fark sayisi.
    public int BreakingCount { get; internal set; }

    // Owned bulgulardan terminal geciste hesaplanan kirici-olmayan fark sayisi.
    public int NonBreakingCount { get; internal set; }

    // Owned bulgulardan terminal geciste hesaplanan yalniz dokumantasyon farki sayisi.
    public int DocsOnlyCount { get; internal set; }

    // Raporun kendine yeterli ve agir owned JSON govdesi.
    public ContractCheckFindings Findings { get; internal set; } = ContractCheckFindings.Empty();

    // Run'in ABP tenant sahipligi.
    public Guid? TenantId { get; internal set; }

    // EF Core materializasyonu icin parametresiz ctor.
    protected ContractCheckRun()
    {
    }

    // Yeni run alanlarini manager tarafindan dogrulanmis degerlerle atar.
    public ContractCheckRun(
        Guid id,
        Guid baseSnapshotId,
        Guid targetSnapshotId,
        Guid pendingStatusId,
        Guid? tenantId)
        : base(id)
    {
        BaseSnapshotId = baseSnapshotId;
        TargetSnapshotId = targetSnapshotId;
        CheckRunStatusId = pendingStatusId;
        TenantId = tenantId;
    }
}
