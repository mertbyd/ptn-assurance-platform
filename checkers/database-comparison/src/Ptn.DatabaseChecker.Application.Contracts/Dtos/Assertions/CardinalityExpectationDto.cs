using Ptn.DatabaseChecker.Constants.Comparison;

namespace Ptn.DatabaseChecker.Dtos.Assertions;

// islevi: Count/row/absence assertion'inin beklenen satir sayisi iliskisini API girdisi olarak tasir.
// sistemdeki gorevi: Uc endpoint'in ayni domain cardinality modeline map edilmesini saglar.
public class CardinalityExpectationDto
{
    public string KindCode { get; set; } = CardinalityKindCodes.Exactly;
    public long ExpectedCount { get; set; } = 1;
}
