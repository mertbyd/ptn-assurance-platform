namespace Ptn.TestModule.Dtos.Bridge.Api;

// islevi: Tek JSON pointer icin turetilebilirlik hukumunu tasir.
// sistemdeki gorevi: Assertion sonucunu kapali outcome koduyla sunar.
public sealed class DerivabilityItemDto
{
    /// <summary>
    /// Ilgili verinin kararli yol veya pointer adresini belirtir.
    /// </summary>
    public string JsonPointer { get; set; } = string.Empty;
    /// <summary>
    /// Degerin yayinlanan kontrollu sozluk kodunu belirtir.
    /// </summary>
    public string OutcomeCode { get; set; } = string.Empty;
}
