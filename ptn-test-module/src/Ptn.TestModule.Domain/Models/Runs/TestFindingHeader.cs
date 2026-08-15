using System;

namespace Ptn.TestModule.Models.Runs;

// islevi: Bulgu listesinin veritabaninda projekte edilen hafif satirini tasir.
// sistemdeki gorevi: Deger ve kanit govdelerini sorgu yuzeyinden disarida tutar.
public sealed class TestFindingHeader
{
    public Guid Id { get; set; }
    public Guid TestRunId { get; set; }
    public Guid? ScenarioId { get; set; }
    public Guid TestRunResultId { get; set; }
    public int Attempt { get; set; }
    public string OutcomeCode { get; set; } = string.Empty;
    public string? SeverityCode { get; set; }
    public int Ordinal { get; set; }
    public string Fingerprint { get; set; } = string.Empty;
    public string SourceCheckerCode { get; set; } = string.Empty;
    public string ComparisonKindCode { get; set; } = string.Empty;
    public string? RuleRef { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreationTime { get; set; }
}
