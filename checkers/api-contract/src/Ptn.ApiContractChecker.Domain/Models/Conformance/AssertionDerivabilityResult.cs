using System.Text.Json;
using Ptn.ApiContractChecker.Constants.Conformance;

namespace Ptn.ApiContractChecker.Models.Conformance;

// islevi: Assertion yollarinin deger tasimayan ve 512 baytlik G2 sonucunu tasir.
// sistemdeki gorevi: Hatali implementasyondan oracle uydurulmasini contract snapshot ile yapisal olarak engeller.
public class AssertionDerivabilityResult
{
    public List<AssertionDerivabilityItem> Assertions { get; set; } = [];
    public bool IsTruncated { get; set; }

    // islevi: Assertion listesini kararli sondan kirparak 512 bayt tavanini uygular.
    public void TrimToBudget()
    {
        while (MeasureUtf8Bytes() > ConformanceAuthoringConstants.MaxAssertionResultBytes && Assertions.Count > 0)
        {
            Assertions.RemoveAt(Assertions.Count - 1);
            IsTruncated = true;
        }
    }

    // islevi: G2 sonucunun UTF-8 JSON boyutunu olcer.
    public int MeasureUtf8Bytes() => JsonSerializer.SerializeToUtf8Bytes(this).Length;
}
