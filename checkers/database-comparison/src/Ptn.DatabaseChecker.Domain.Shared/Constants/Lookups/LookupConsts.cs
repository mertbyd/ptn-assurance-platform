namespace Ptn.DatabaseChecker.Constants.Lookups;

// islevi: Tum lookup kolonlarinin uzunluk sinirlarini tek kaynakta tanimlar.
// sistemdeki gorevi: EF mapping (HasMaxLength) ve FluentValidation (MaximumLength) ayni sabiti kullanir; sema ile validasyon asla birbirinden kaymaz.
public static class LookupConsts
{
    // Kararli kod alaninin azami uzunlugu; kisa tutulur cunku teknik anahtar, cumle degil.
    public const int MaxCodeLength = 64;

    // Insan-okur ad alaninin azami uzunlugu.
    public const int MaxNameLength = 128;

    // Opsiyonel aciklama alaninin azami uzunlugu.
    public const int MaxDescriptionLength = 512;
}
