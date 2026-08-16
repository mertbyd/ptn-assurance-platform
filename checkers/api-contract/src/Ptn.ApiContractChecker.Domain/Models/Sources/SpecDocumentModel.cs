namespace Ptn.ApiContractChecker.Models.Sources;

// islevi: SpecSource aggregate'i icindeki bir dokuman taniminin mutasyon girdisini tasir.
// sistemdeki gorevi: DTO'yu Domain'e sokmadan dokuman ekleme, guncelleme ve pasiflestirme niyetini manager'a iletir.
public class SpecDocumentModel
{
    // Mevcut dokumanda kimlik; Guid.Empty yeni dokuman ekleme niyetidir.
    public Guid Id { get; set; }

    // Kaynak icindeki benzersiz dokuman adi.
    public string DocumentName { get; set; } = default!;

    // Kaynak taban adresine goreli dokuman yolu.
    public string Path { get; set; } = default!;

    // False deger dokumani fiziksel silmeden pasiflestirir.
    public bool IsActive { get; set; } = true;
}
