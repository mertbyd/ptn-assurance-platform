namespace Ptn.TestModule.Dtos.Runs;

// islevi: Dis sistemden gelen webhook kosum tetiginin idempotency ve hedef alanlarini tasir.
// sistemdeki gorevi: Ayni teslim kimliginin ikinci kez kosum uretmemesini saglayan public sozlesmedir.
/// <summary>Webhook ile kosum tetikleme girdisidir.</summary>
public class WebhookTestRunDto
{
    /// <summary>Gonderen sistemin teslim kimligidir; tetikleyici referansi olarak saklanir.</summary>
    public string DeliveryId { get; set; } = string.Empty;

    /// <summary>Kosturulacak yayinlanmis senaryonun kararli anahtaridir.</summary>
    public string ScenarioKey { get; set; } = string.Empty;

    /// <summary>Istege bagli mantiksal ortam anahtaridir; verilmezse otomasyon ayari kullanilir.</summary>
    public string? EnvironmentKey { get; set; }
}
