namespace Ptn.DatabaseChecker.Dtos.Assertions;

// islevi: Basarisiz kolon matcher'inin kucuk ve redaction uygulanmis API sonucudur.
// sistemdeki gorevi: Test Module'a hangi beklentinin neden eslesmedigini ham hedef satiri acmadan bildirir.
public class FailedExpectationDto
{
    public string ColumnName { get; set; } = string.Empty;
    public string MatcherKindCode { get; set; } = string.Empty;
    public string? ExpectedValue { get; set; }
    public string? ObservedValue { get; set; }
}
