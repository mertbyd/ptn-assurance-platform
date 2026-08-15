namespace Ptn.TestModule.Models.Runs;

// islevi: Webhook kosum tetiginin Manager girdisini tasir.
// sistemdeki gorevi: Public DTO'yu domain kararindan ayirir; sir degeri bu modelde tasinmaz.
public class WebhookRunTriggerModel
{
    public string DeliveryId { get; set; } = string.Empty;
    public string ScenarioKey { get; set; } = string.Empty;
    public string? EnvironmentKey { get; set; }
}
