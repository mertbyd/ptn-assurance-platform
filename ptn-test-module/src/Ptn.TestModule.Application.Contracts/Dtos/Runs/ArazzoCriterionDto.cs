namespace Ptn.TestModule.Dtos.Runs;

// islevi: Tek bir degerlendirme kriterinin turunu ve ifadesini wire seklinde tasir.
// sistemdeki gorevi: Runner'in desteklemedigi kriter turunun tespitine ham veri verir.
public class ArazzoCriterionDto
{
    /// <summary>Kriterin degerlendirme turudur.</summary>
    public string? Type { get; set; }

    /// <summary>Kriterin degerlendirilecek ifadesidir.</summary>
    public string? Condition { get; set; }

    /// <summary>Ifadenin degerlendirildigi baglamdir.</summary>
    public string? Context { get; set; }
}
