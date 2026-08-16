namespace Ptn.DatabaseChecker.Models.Comparison;

// islevi: Assertion deger eslestirmesi icin tablonun tek kolonuna ait kanonik tip ve hassasiyet bilgisini tasir.
// sistemdeki gorevi: Schema snapshot tip semantigini provider SQL'inden ayirip ValueMatcherEvaluator'a aktarir.
public sealed class TableDataColumnModel
{
    public string Name { get; set; } = string.Empty;
    public string CanonicalDataTypeCode { get; set; } = string.Empty;
    public int? NumericScale { get; set; }
}
