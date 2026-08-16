namespace Ptn.DatabaseChecker.Constants.Comparison;

// islevi: ComparisonDefinition kolonlarinin uzunluk sinirlarini tek kaynakta tanimlar.
// sistemdeki gorevi: EF mapping ve FluentValidation ayni sabiti kullanir; sema ile validasyon birbirinden kaymaz.
public static class ComparisonDefinitionConsts
{
    // Is tarifinin insan-okur adinin azami uzunlugu ("Test->Canli Gunluk Kontrol").
    public const int MaxNameLength = 128;

    // Tarif aciklamasinin azami uzunlugu.
    public const int MaxDescriptionLength = 512;

    /// <summary>Kaynak taraf rol kodunun azami uzunlugu.</summary>
    public const int MaxSourceRoleCodeLength = 16;
}
