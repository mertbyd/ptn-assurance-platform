namespace Ptn.TestModule.Models.Bridge.Invariants;

// islevi: Is degismezi yoklamasinin kapali gecti/kaldi kararini ve gerekce kodunu tasir.
// sistemdeki gorevi: Kararin serbest metin aciklamaya donusmesini engeller.
public sealed class BusinessInvariantResult
{
    public bool Passed { get; set; }
    public string ReasonCode { get; set; } = string.Empty;
}
