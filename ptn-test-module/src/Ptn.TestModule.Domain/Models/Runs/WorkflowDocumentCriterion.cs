namespace Ptn.TestModule.Models.Runs;

// islevi: Tek bir degerlendirme kriterinin turunu ve ifadesini tasir.
// sistemdeki gorevi: Desteklenmeyen kriter turu kararina domain tarafinda veri verir.
/// <summary>
/// Kosum belgesindeki bir degerlendirme kriterini tasir.
/// </summary>
public class WorkflowDocumentCriterion
{
    /// <summary>Kriterin degerlendirme turudur.</summary>
    public string? Type { get; set; }

    /// <summary>Kriterin degerlendirilecek ifadesidir.</summary>
    public string? Condition { get; set; }

    /// <summary>Ifadenin degerlendirildigi baglamdir.</summary>
    public string? Context { get; set; }
}
