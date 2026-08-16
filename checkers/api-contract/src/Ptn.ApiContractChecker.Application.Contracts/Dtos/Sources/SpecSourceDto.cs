using Volo.Abp.Application.Dtos;

namespace Ptn.ApiContractChecker.Dtos.Sources;

// islevi: Kayitli spec kaynaginin secretsiz API cevap modelidir.
// sistemdeki gorevi: Kaynak tanimini ve aggregate dokumanlarini dondurur; credential ve Vault yolu bu sozlesmede bulunmaz.
public class SpecSourceDto : EntityDto<Guid>
{
    // Tenant icindeki insan-okur kaynak adi.
    public string Name { get; set; } = default!;

    // Dokuman yollarinin cozuldugu servis kok adresi.
    public string BaseUrl { get; set; } = default!;

    // Kaynagin yeni kontrollerde secilebilirligini belirtir.
    public bool IsActive { get; set; }

    // Aggregate'e ait aktif dokuman tanimlari.
    public List<SpecDocumentDto> Documents { get; set; } = [];
}
