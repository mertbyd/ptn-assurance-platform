using System;

namespace Ptn.TestModule.Models.Runs;

// islevi: Kosum liste filtre, siralama ve sayfalama degerlerini tasir.
// sistemdeki gorevi: Application.Contracts bagimliligi olmadan repository sorgusunu tipler.
public sealed class TestRunQuery
{
    public string? RunStatusCode { get; set; }
    public string? EnvironmentKey { get; set; }
    public Guid? ScenarioId { get; set; }
    public string? TriggerKindCode { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public bool? IsDryRun { get; set; }
    public string? Sorting { get; set; }
    public int SkipCount { get; set; }
    public int MaxResultCount { get; set; }
}
