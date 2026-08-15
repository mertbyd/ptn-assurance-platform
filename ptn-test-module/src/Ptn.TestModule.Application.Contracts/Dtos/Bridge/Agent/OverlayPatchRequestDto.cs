namespace Ptn.TestModule.Dtos.Bridge;

// islevi: OpenAPI Overlay onerisi icin bulgu, hedef ve guncelleme girdisini tasir.
// sistemdeki gorevi: Gerekcesiz veya bulgusuz yamayi public sinirda imkansiz kilar.
public sealed class OverlayPatchRequestDto
{
    public string FindingFingerprint { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string UpdateJson { get; set; } = string.Empty;
}
