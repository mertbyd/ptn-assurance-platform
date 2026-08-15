using System;
using Volo.Abp.Application.Dtos;

namespace Ptn.TestModule.Dtos.Runs;

// islevi: Bulgu listesinin agir kanit govdesi tasimayan basligini tanimlar.
// sistemdeki gorevi: UI'in kosumlar arasi bulgu ekranini sayfali kurmasini saglar.
public sealed class TestFindingHeaderDto : EntityDto<Guid>
{
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
