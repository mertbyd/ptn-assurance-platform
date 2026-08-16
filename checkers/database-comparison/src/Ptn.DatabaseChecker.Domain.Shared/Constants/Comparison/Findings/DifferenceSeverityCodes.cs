namespace Ptn.DatabaseChecker.Constants.Comparison;

// islevi: Karsilastirma bulgularinin uyumluluk etkisini belirleyen kararli siddet kodlarini tanimlar.
// sistemdeki gorevi: Saf siniflandirici, owned JSON bulgulari ve MCP filtreleme kontrati ayni kod sozlugunu kullanir.
/// <summary>
/// Karsilastirma bulgularinin kararli siddet kodlari.
/// </summary>
public static class DifferenceSeverityCodes
{
    /// <summary>Geriye uyumsuz fark.</summary>
    public const string Breaking = "Breaking";
    /// <summary>Geriye uyumlu fark.</summary>
    public const string NonBreaking = "NonBreaking";
    /// <summary>Insan incelemesi gerektiren fark.</summary>
    public const string Warning = "Warning";
    /// <summary>Yalniz dokumantasyon farki.</summary>
    public const string DocsOnly = "DocsOnly";

    /// <summary>Tanimli tum siddet kodlari.</summary>
    public static IReadOnlyCollection<string> All { get; } =
        [Breaking, NonBreaking, Warning, DocsOnly];

    /// <summary>En agirdan en hafife dogru kararli siddet oncelik sirasi.</summary>
    public static IReadOnlyList<string> Ranked { get; } =
        [Breaking, NonBreaking, Warning, DocsOnly];

    /// <summary>
    /// Kodun kapali siddet katalogunda bulunup bulunmadigini bildirir.
    /// </summary>
    public static bool IsDefined(string? code)
        => code is Breaking or NonBreaking or Warning or DocsOnly;
}
