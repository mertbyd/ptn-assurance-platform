namespace Ptn.TestModule.Constants.Runs;

// islevi: Terminal bulgu satirinin metin ve indeks sinirlarini tanimlar.
// sistemdeki gorevi: Checker kanitini kesmeden saklayan Manager ve EF sozlesmesini DBML ile hizalar.
/// <summary>
/// Test sonucu bulgularinin kararli uzunluk ve indeks sabitlerini tasir.
/// </summary>
public static class TestResultFindingConsts
{
    /// <summary>Makine-okur bulgu konumunun azami karakter sayisidir.</summary>
    public const int MaxLocationLength = 1000;

    /// <summary>Hedef gorunen adinin azami karakter sayisidir.</summary>
    public const int MaxTargetDisplayNameLength = 256;

    /// <summary>Bulgu mesajinin azami karakter sayisidir.</summary>
    public const int MaxMessageLength = 1000;

    /// <summary>Beklenen veya gozlenen degerin azami karakter sayisidir.</summary>
    public const int MaxValueLength = 2000;

    /// <summary>Kanit ozetinin azami karakter sayisidir.</summary>
    public const int MaxEvidenceSummaryLength = 2000;

    /// <summary>Acik uclu tur kodlarinin azami karakter sayisidir.</summary>
    public const int MaxKindCodeLength = 64;

    /// <summary>Is kurali referansinin azami karakter sayisidir.</summary>
    public const int MaxRuleRefLength = 64;

    /// <summary>Aggregate icindeki kararli bulgu sirasi unique indeks adidir.</summary>
    public const string OrderIndexName = "ux_findings_order";

    /// <summary>Bulgu konumu sorgularinin indeks adidir.</summary>
    public const string LocationIndexName = "ix_findings_loc";

    /// <summary>Is kurali kapsami sorgularinin indeks adidir.</summary>
    public const string RuleIndexName = "ix_findings_rule";

    /// <summary>Kaynak checker sorgularinin indeks adidir.</summary>
    public const string SourceIndexName = "ix_findings_src";
}
