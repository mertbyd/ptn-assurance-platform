namespace Ptn.ApiContractChecker.Dtos.Lookups;

// islevi: Tum lookup create DTO'lari icin ortak alanlari tanimlar.
// sistemdeki gorevi: Yeni lookup modulu eklendiginde Code/Name/Description/IsActive tekrarini engeller.
public abstract class LookupCreateDto
{
    /// <summary>
    /// Yeni lookup satirinin kararli teknik kodu.
    /// </summary>
    public string Code { get; set; } = default!;

    /// <summary>
    /// Yeni lookup satirinin ekranda gosterilecek insan-okur adi.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Yeni lookup satirinin opsiyonel aciklamasi.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Yeni lookup satirinin baslangic aktiflik durumu.
    /// </summary>
    public bool IsActive { get; set; }
}
