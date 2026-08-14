namespace Ptn.TestModule.Dtos.Bridge.Api;

// islevi: Tek JSON pointer icin turetilebilirlik hukumunu tasir.
// sistemdeki gorevi: Assertion sonucunu kapali outcome koduyla sunar.
public sealed class DerivabilityItemDto
{
    public string JsonPointer { get; set; } = string.Empty;
    public string OutcomeCode { get; set; } = string.Empty;
}
