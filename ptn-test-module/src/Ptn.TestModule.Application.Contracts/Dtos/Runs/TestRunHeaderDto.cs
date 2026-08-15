using System;
using Volo.Abp.Application.Dtos;

namespace Ptn.TestModule.Dtos.Runs;

// islevi: Kosum listesinin agir kolon tasimayan zengin basligini tanimlar.
// sistemdeki gorevi: UI'in hukum, sure ve bulgu sayisini tek sorguda kurmasini saglar.
public sealed class TestRunHeaderDto : EntityDto<Guid>
{
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
