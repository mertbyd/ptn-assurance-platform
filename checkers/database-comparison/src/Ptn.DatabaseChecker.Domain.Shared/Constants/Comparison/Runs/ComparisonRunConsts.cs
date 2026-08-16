namespace Ptn.DatabaseChecker.Constants.Comparison;

// islevi: ComparisonRun kolonlarinin uzunluk sinirlarini tek kaynakta tanimlar.
// sistemdeki gorevi: EF mapping ve FluentValidation ayni sabiti kullanir; sema ile validasyon birbirinden kaymaz.
public static class ComparisonRunConsts
{
    // Failed durumundaki kullaniciya gosterilecek hata mesajinin azami uzunlugu.
    public const int MaxErrorMessageLength = 2048;

    /// <summary>MCP bulgu okumasinin varsayilan sayfa boyutu.</summary>
    public const int DefaultFindingPageSize = 20;

    /// <summary>MCP bulgu okumasinin ayar yokken kullanilan azami sayfa boyutu.</summary>
    public const int DefaultMaxFindingPageSize = 100;

    /// <summary>MCP bulgu cevabinin ayar yokken kullanilan UTF-8 byte butcesi.</summary>
    public const int DefaultFindingResponseBytes = 32 * 1024;

    /// <summary>Result ve ABP transport zarfi icin bulgu sayfasi butcesinden ayrilan byte payi.</summary>
    public const int FindingResponseEnvelopeReserveBytes = 512;

    /// <summary>Uc bulgu ailesinin count ve sinirli pencere okumalarinin toplam round-trip sayisi.</summary>
    public const int FindingQueryRoundTripCount = 6;
    public const int FindingSinceQueryRoundTripCount = 9;

    /// <summary>Tek bulgu sorgusunda kabul edilen azami fingerprint sayisi.</summary>
    public const int MaxFindingFingerprintFilterCount = 100;

    /// <summary>SHA-256 fingerprint'in hexadecimal karakter sayisi.</summary>
    public const int FindingFingerprintHexLength = 64;

    /// <summary>Public fingerprint filtrelerinin kabul ettigi SHA-256 hexadecimal deseni.</summary>
    public const string FindingFingerprintPattern = "^[0-9A-Fa-f]{64}$";
}
