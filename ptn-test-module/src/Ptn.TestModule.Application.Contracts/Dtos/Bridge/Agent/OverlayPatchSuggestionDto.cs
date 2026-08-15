namespace Ptn.TestModule.Dtos.Bridge;

// islevi: Standart OpenAPI Overlay dokumanini bulgu bagi ile public tasir.
// sistemdeki gorevi: Onerinin uygulanmadigini acik bir alanla korur.
public sealed class OverlayPatchSuggestionDto
{
    public string FindingFingerprint { get; set; } = string.Empty;
    public string Document { get; set; } = string.Empty;
    public bool Applied { get; set; }
}
