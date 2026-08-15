namespace Ptn.TestModule.Models.Bridge.Agent;

// islevi: Bulguyla bagli OpenAPI Overlay dokumanini tasir.
// sistemdeki gorevi: Yama onerisi ile uygulama eylemini ayirip otomatik uygulamayi imkansiz kilar.
public class OverlayPatchSuggestion
{
    public string FindingFingerprint { get; set; } = string.Empty;
    public string Document { get; set; } = string.Empty;
    public bool Applied { get; set; }
}
