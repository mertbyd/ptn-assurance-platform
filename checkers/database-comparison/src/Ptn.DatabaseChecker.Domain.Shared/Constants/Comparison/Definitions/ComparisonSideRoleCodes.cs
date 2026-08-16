namespace Ptn.DatabaseChecker.Constants.Comparison;

// islevi: ComparisonDefinition kaynak tarafinin Reference veya Audited rolunu kararli kodla tanimlar.
// sistemdeki gorevi: Fark yonunu uyumluluk etkisine cevirirken kaynak/hedef adlarini is rolunden ayiran tek sozluktur.
/// <summary>
/// Karsilastirma taraflarinin kararli is rolu kodlari.
/// </summary>
public static class ComparisonSideRoleCodes
{
    /// <summary>Beklenen dogru durumu tasiyan taraf.</summary>
    public const string Reference = "Reference";
    /// <summary>Reference durumuna gore denetlenen taraf.</summary>
    public const string Audited = "Audited";

    /// <summary>Tanimli tum taraf rolu kodlari.</summary>
    public static IReadOnlyCollection<string> All { get; } = [Reference, Audited];

    /// <summary>
    /// Kodun kapali taraf rolu katalogunda bulunup bulunmadigini bildirir.
    /// </summary>
    public static bool IsDefined(string? code)
        => code is Reference or Audited;

    /// <summary>
    /// Kaynak taraf rolunun karsisindaki hedef taraf rolunu dondurur.
    /// </summary>
    public static string Opposite(string sourceRoleCode)
        => sourceRoleCode == Reference ? Audited : Reference;
}
