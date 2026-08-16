namespace Ptn.TestModule.Models.Authoring;

// islevi: Yeni yazarlik oturumunun workflow ve sourceDescription girdilerini tasir.
// sistemdeki gorevi: Public DTO'yu cache session kurallarindan ayiran Mapperly hedefidir.
public sealed class AuthoringSessionCreateModel
{
    public string WorkflowId { get; set; } = string.Empty;
    public string WorkflowSummary { get; set; } = string.Empty;
    public string ApiSourceUrl { get; set; } = string.Empty;
    public string DatabaseSourceUrl { get; set; } = string.Empty;
}
