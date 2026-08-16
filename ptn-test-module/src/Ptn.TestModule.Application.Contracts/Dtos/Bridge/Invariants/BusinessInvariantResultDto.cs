namespace Ptn.TestModule.Dtos.Bridge.Invariants;

// islevi: Is degismezi yoklamasinin kapali kararini ve gerekce kodunu tasir.
// sistemdeki gorevi: Kararin serbest metin aciklamaya donusmesini engeller.
public sealed class BusinessInvariantResultDto
{
    /// <summary>Degismezin saglanip saglanmadigini belirtir.</summary>
    public bool Passed { get; set; }

    /// <summary>Kararin kapali gerekce kodunu belirtir.</summary>
    public string ReasonCode { get; set; } = string.Empty;
}
