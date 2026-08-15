namespace Ptn.TestModule.Models.Runs;

// islevi: Bir checker veya runner bulgusunun kalici yazim oncesi alanlarini tasir.
// sistemdeki gorevi: TestRunResultManager'in siralama ve guvenli uzunluk kapilarina domain girdisi verir.
/// <summary>
/// Terminal sonuca eklenecek tek bir test bulgusunu tasir.
/// </summary>
public class TestResultFindingModel
{
    /// <summary>Rapor icindeki istenen kararli sira numarasidir.</summary>
    public int Ordinal { get; set; }

    /// <summary>Bulguyu ureten checker veya runner kodudur.</summary>
    public string SourceCheckerCode { get; set; } = string.Empty;

    /// <summary>Kaynak checker'in acik uclu karsilastirma turu kodudur.</summary>
    public string ComparisonKindCode { get; set; } = string.Empty;

    /// <summary>Dogrulanan is kurali referansidir.</summary>
    public string? RuleRef { get; set; }

    /// <summary>Farkin makine-okur tam konumudur.</summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>Kullaniciya gosterilecek hedef adidir.</summary>
    public string? TargetDisplayName { get; set; }

    /// <summary>Bulgunun kisa SARIF uyumlu mesajidir.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Beklenen degerin guvenli metin temsilidir.</summary>
    public string? ExpectedValue { get; set; }

    /// <summary>Gozlenen degerin guvenli metin temsilidir.</summary>
    public string? ObservedValue { get; set; }

    /// <summary>Buyuk kanit govdesi yerine satir icinde tutulan ozettir.</summary>
    public string? EvidenceSummary { get; set; }

    /// <summary>Asenkron gozlemin kosum baslangicina gore milisaniye konumudur.</summary>
    public int? ObservedAtMs { get; set; }

    /// <summary>Polling sirasinda yapilan deneme sayisidir.</summary>
    public int? AttemptCount { get; set; }
}
