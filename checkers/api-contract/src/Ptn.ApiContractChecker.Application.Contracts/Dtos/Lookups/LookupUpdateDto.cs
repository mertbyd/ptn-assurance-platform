namespace Ptn.ApiContractChecker.Dtos.Lookups;

// islevi: Tum lookup update DTO'lari icin ortak alanlari tanimlar.
// sistemdeki gorevi: Kararli Code'u API update yuzeyinden uzak tutarak yeni lookup modullerinin yalniz gorunen ve aktiflik alanlarini degistirmesini saglar.
public abstract class LookupUpdateDto
{
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
