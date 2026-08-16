using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Ptn.ApiContractChecker.Entities.Sources;

// islevi: Izlenen bir servisin adresini, Vault referansini ve yayinladigi spec dokumanlarini tasir.
// sistemdeki gorevi: SpecDocument cocuklarinin ince aggregate sahibi ve tenant izolasyon kokudur.
public class SpecSource : AuditedAggregateRoot<Guid>, IPassivable, IMultiTenant
{
    private readonly List<SpecDocument> _documents = [];

    // Kaynagin tenant icindeki insan-okur ve benzersiz adi.
    public string Name { get; internal set; } = default!;

    // Dokuman yollarinin cozuldugu servis kok adresi.
    public string BaseUrl { get; internal set; } = default!;

    // Kimlik bilgisinin Vault adresi; null deger acik spec kaynagini anlatir.
    public string? VaultSecretPath { get; internal set; }

    // Emekli kaynak silinmez; gecmis snapshot ve run referanslari korunur.
    public bool IsActive { get; internal set; }

    // ABP tenant filtresinin kullandigi sahiplik kimligi.
    public Guid? TenantId { get; internal set; }

    // Aggregate'e ait dokuman tanimlarini dis katmanlara salt okunur acar.
    public IReadOnlyCollection<SpecDocument> Documents => _documents.AsReadOnly();

    // Aggregate koleksiyonunu yalniz ayni assembly'deki manager davranisina acar.
    internal List<SpecDocument> MutableDocuments => _documents;

    // EF Core materializasyonu icin parametresiz ctor.
    protected SpecSource()
    {
    }

    // Kaynak alanlarini davranis uygulamadan atar; kanoniklestirme ve tekillik manager'a aittir.
    public SpecSource(
        Guid id,
        string name,
        string baseUrl,
        string? vaultSecretPath,
        Guid? tenantId,
        bool isActive = true)
        : base(id)
    {
        Name = name;
        BaseUrl = baseUrl;
        VaultSecretPath = vaultSecretPath;
        TenantId = tenantId;
        IsActive = isActive;
    }
}
