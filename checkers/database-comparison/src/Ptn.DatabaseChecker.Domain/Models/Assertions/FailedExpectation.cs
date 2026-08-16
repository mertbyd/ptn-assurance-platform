namespace Ptn.DatabaseChecker.Models.Assertions;

// islevi: Basarisiz tek kolon beklentisinin matcher ve redaction uygulanmis deger ozetini tasir.
// sistemdeki gorevi: Test Module'a tum satiri donmeden hedefli hata kaniti verir; ham kaynak deger retention politikasini asamaz.
public sealed class FailedExpectation
{
    public string ColumnName { get; set; } = string.Empty;
    public string MatcherKindCode { get; set; } = string.Empty;
    public string? ExpectedValue { get; set; }
    public string? ObservedValue { get; set; }
}
