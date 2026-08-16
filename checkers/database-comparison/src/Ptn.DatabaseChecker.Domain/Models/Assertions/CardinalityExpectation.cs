using Ptn.DatabaseChecker.Constants.Comparison;

namespace Ptn.DatabaseChecker.Models.Assertions;

// islevi: Bir anahtar sorgusundan beklenen satir sayisi iliskisini tasir.
// sistemdeki gorevi: Row, count ve absent uclarinin ayni manager cekirdeginde farkli cardinality semantigiyle calismasini saglar.
public sealed class CardinalityExpectation
{
    public string KindCode { get; set; } = CardinalityKindCodes.Exactly;
    public long ExpectedCount { get; set; } = 1;
}
