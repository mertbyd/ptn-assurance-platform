using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Ptn.ApiContractChecker.Entities.Sources;

// islevi: Bir SpecSource'un yayinladigi adlandirilmis ve goreli yollu spec dokumanini temsil eder.
// sistemdeki gorevi: Snapshot'larin hangi kaynak dokumanina ait oldugunu belirleyen ince aggregate cocugudur.
public class SpecDocument : AuditedEntity<Guid>, IPassivable, IMultiTenant
{
    // Dokumanin sahibi aggregate kokunun kimligi.
    public Guid SpecSourceId { get; internal set; }

    // Kaynak icindeki insan-okur ve benzersiz dokuman adi.
    public string DocumentName { get; internal set; } = default!;

    // SpecSource.BaseUrl'e goreli dokuman yolu.
    public string Path { get; internal set; } = default!;

    // Yayindan kalkan dokuman silinmez ve gecmis snapshot'lar korunur.
    public bool IsActive { get; internal set; }

    // Dokumanin zamanlanmis taramaya girip girmedigi.
    public bool IsMonitored { get; internal set; }

    // Iki zamanlanmis kontrol arasindaki bekleme.
    public int? CheckIntervalMinutes { get; internal set; }

    // Worker'in vade kararini verdigi zaman damgasi.
    public DateTime? NextCheckAt { get; internal set; }

    // Son zamanlanmis kontrol denemesinin zamani.
    public DateTime? LastCheckedAt { get; internal set; }

    // Zamanlanmis cekimin son degisim veya erisim sonucu.
    public string? LastFetchOutcome { get; internal set; }

    // Aggregate kokuyla ayni ABP tenant sahipligini tasir.
    public Guid? TenantId { get; internal set; }

    // EF Core materializasyonu icin parametresiz ctor.
    protected SpecDocument()
    {
    }

    // Yeni dokuman alanlarini davranis uygulamadan atar; kurallar SpecSourceManager'a aittir.
    internal SpecDocument(
        Guid id,
        Guid specSourceId,
        string documentName,
        string path,
        Guid? tenantId,
        bool isActive = true)
        : base(id)
    {
        SpecSourceId = specSourceId;
        DocumentName = documentName;
        Path = path;
        TenantId = tenantId;
        IsActive = isActive;
    }
}
