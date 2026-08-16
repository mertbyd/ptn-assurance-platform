namespace Ptn.DatabaseChecker.Dtos.Lookups;

// islevi: Tum lookup update DTO'lari icin ortak alanlari tanimlar.
// sistemdeki gorevi: Yeni lookup modulu eklendiginde guncellenebilir lookup alanlarini standartlastirir.
public abstract class LookupUpdateDto
{
    /// <summary>
    /// Lookup satirinin guncellenecek kararli teknik kodu.
    /// </summary>
    public string Code { get; set; } = default!;

    /// <summary>
    /// Lookup satirinin guncellenecek insan-okur adi.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Lookup satirinin guncellenecek opsiyonel aciklamasi.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Lookup satirinin guncellenecek aktiflik durumu.
    /// </summary>
    public bool IsActive { get; set; }
}
