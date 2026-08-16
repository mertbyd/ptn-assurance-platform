namespace Ptn.DatabaseChecker.Models.Lookups;

// islevi: Her lookup icin yeni satir olustururken domain katmaninin ihtiyac duydugu ortak alanlari tasir.
// sistemdeki gorevi: Tum lookup create akislarinda ayni modelin kullanilmasini saglar; her lookup ayni alan setine sahip oldugu icin tek model yeterlidir (golden rule 1: is bir kez yapilir).
public class LookupCreateModel
{
    // Kararli teknik kod; benzersizligi manager LookupManager tarafindan dogrulanir.
    public string Code { get; set; } = default!;

    // Insan-okur ad; UI listelerinde gosterilir.
    public string Name { get; set; } = default!;

    // Opsiyonel aciklama; satirin kapsamini anlatir.
    public string? Description { get; set; }

    // Satirin operasyonel olarak secilebilir olup olmadigi (IPassivable karsiligi).
    public bool IsActive { get; set; }
}
