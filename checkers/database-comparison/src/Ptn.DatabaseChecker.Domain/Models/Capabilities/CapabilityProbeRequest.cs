namespace Ptn.DatabaseChecker.Models.Capabilities;

// islevi: Yazma kumesi yoklamasinin cagiran tarafindan bildirilen sandbox tekillik olgusunu tasir.
// sistemdeki gorevi: Public DTO'yu Domain kararindan ayirarak probe siralamasinin Manager'da kalmasini saglar.
public sealed class CapabilityProbeRequest
{
    public bool RequiresExclusiveSandbox { get; set; }
}
