using System;
using System.Collections.Generic;

namespace Ptn.TestModule.Models.Runs;

// islevi: Kosumlar arasi bulgu sorgusunun tipli filtrelerini tasir.
// sistemdeki gorevi: Repository'nin DTO bagimliligi almadan bounded query uygulamasini saglar.
public sealed class TestFindingQuery
{
    public Guid? TestRunId { get; set; }
    public Guid? ScenarioId { get; set; }
    public string? OutcomeCode { get; set; }
    public string? SeverityCode { get; set; }
    public string? SourceCheckerCode { get; set; }
    public string? RuleRef { get; set; }
    public IReadOnlyCollection<string> Fingerprints { get; set; } = [];
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public string? Sorting { get; set; }
    public int SkipCount { get; set; }
    public int MaxResultCount { get; set; }
}
