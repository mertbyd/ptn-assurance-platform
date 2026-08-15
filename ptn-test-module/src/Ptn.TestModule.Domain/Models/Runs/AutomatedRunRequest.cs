using System;

namespace Ptn.TestModule.Models.Runs;

// islevi: Zamanlama, webhook ve sozlesme degisikligi tetikleyicilerinin ortak kosum istegini tasir.
// sistemdeki gorevi: Uc otomatik yolun ayni idempotency ve ortam kararini kullanmasini saglar.
public class AutomatedRunRequest
{
    public Guid ScenarioId { get; set; }
    public string ScenarioKey { get; set; } = string.Empty;
    public string TriggerKindCode { get; set; } = string.Empty;
    public string TriggerRef { get; set; } = string.Empty;
    public string EnvironmentKey { get; set; } = string.Empty;
    public string CanonicalInputs { get; set; } = string.Empty;

    // Idempotency kapisinin senaryo boyutunu dikkate alip almayacagini belirler.
    public bool IsScenarioScopedTrigger { get; set; } = true;
}
