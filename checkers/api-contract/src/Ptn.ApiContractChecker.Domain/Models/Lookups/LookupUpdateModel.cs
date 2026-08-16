namespace Ptn.ApiContractChecker.Models.Lookups;

// islevi: Her lookup icin mevcut satiri guncellerken domain katmaninin ihtiyac duydugu ortak alanlari tasir.
// sistemdeki gorevi: Kararli Code'u degisiklik yuzeyine almadan tum lookup update akislarinin gorunen ve aktiflik alanlarini standartlastirir.
public class LookupUpdateModel
{
    // Guncellenen insan-okur ad.
    public string Name { get; set; } = default!;

    // Guncellenen opsiyonel aciklama.
    public string? Description { get; set; }

    // Satirin aktiflik durumu (pasife alma niyeti IPassivable ile yurur).
    public bool IsActive { get; set; }
}
