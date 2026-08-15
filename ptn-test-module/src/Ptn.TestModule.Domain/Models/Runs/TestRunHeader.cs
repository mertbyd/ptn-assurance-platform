using System;

namespace Ptn.TestModule.Models.Runs;

// islevi: Kosum listesinin veritabaninda projekte edilen hafif basligini tasir.
// sistemdeki gorevi: Agir aggregate kolonlarini Application katmanina cikarmadan liste sonucunu verir.
public sealed class TestRunHeader
{
    public Guid Id { get; set; }
    public Guid? ScenarioId { get; set; }
    public string TestKey { get; set; } = string.Empty;
    public string EnvironmentKey { get; set; } = string.Empty;
    public string RunStatusCode { get; set; } = string.Empty;
    public string? OutcomeCode { get; set; }
    public string TriggerKindCode { get; set; } = string.Empty;
    public int? DurationMs { get; set; }
    public int FindingCount { get; set; }
    public int? Attempt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsDryRun { get; set; }
    public DateTime CreationTime { get; set; }
}
