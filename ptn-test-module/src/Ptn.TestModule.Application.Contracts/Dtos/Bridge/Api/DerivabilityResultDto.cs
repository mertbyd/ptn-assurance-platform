namespace Ptn.TestModule.Dtos.Bridge.Api;

// islevi: Assertion turetilebilirlik hukumlerini ve kesilme durumunu tasir.
// sistemdeki gorevi: Normalize sonucu public Bridge cevabina tasir.
public sealed class DerivabilityResultDto
{
    /// <summary>
    /// Ilgili degerleri kararli sirada listeler.
    /// </summary>
    public List<DerivabilityItemDto> Assertions { get; set; } = [];
    /// <summary>
    /// Ilgili yetenek, sonuc veya durumun etkin olup olmadigini belirtir.
    /// </summary>
    public bool IsTruncated { get; set; }
}
