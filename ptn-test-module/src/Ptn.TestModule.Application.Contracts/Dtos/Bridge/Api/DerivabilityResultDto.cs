namespace Ptn.TestModule.Dtos.Bridge.Api;

// islevi: Assertion turetilebilirlik hukumlerini ve kesilme durumunu tasir.
// sistemdeki gorevi: Normalize sonucu public Bridge cevabina tasir.
public sealed class DerivabilityResultDto
{
    public List<DerivabilityItemDto> Assertions { get; set; } = [];
    public bool IsTruncated { get; set; }
}
