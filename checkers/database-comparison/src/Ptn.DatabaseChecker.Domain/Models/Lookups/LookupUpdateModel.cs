namespace Ptn.DatabaseChecker.Models.Lookups;

// islevi: Her lookup icin mevcut satiri guncellerken domain katmaninin ihtiyac duydugu ortak alanlari tasir.
// sistemdeki gorevi: Tum lookup update akislarinda ayni modelin kullanilmasini saglar; alan seti tum lookup'larda ayni oldugu icin tek model yeterlidir (golden rule 1: is bir kez yapilir).
public class LookupUpdateModel
{
    // Guncellenen kararli teknik kod; degistiginde benzersizligi manager yeniden dogrular.
    public string Code { get; set; } = default!;

    // Guncellenen insan-okur ad.
    public string Name { get; set; } = default!;

    // Guncellenen opsiyonel aciklama.
    public string? Description { get; set; }

    // Satirin aktiflik durumu (pasife alma niyeti IPassivable ile yurur).
    public bool IsActive { get; set; }
}
