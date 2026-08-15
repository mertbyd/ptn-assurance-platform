using System;
using System.Collections.Generic;
using Ptn.TestModule.Constants.Runs;
using Volo.Abp.Application.Dtos;

namespace Ptn.TestModule.Dtos.Runs;

// islevi: Kosumlar arasi bulgu sorgusunun sinirli filtrelerini tanimlar.
// sistemdeki gorevi: Agir bulgu govdelerini liste yuzeyinden ayiran tipli query sozlesmesidir.
public sealed class TestFindingListInput : PagedResultRequestDto
{
    public Guid? TestRunId { get; set; }
    public Guid? ScenarioId { get; set; }
    public string? OutcomeCode { get; set; }
    public string? SeverityCode { get; set; }
    public string? SourceCheckerCode { get; set; }
    public string? RuleRef { get; set; }
    public List<string> Fingerprints { get; set; } = [];
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public string? Sorting { get; set; }

    public TestFindingListInput()
    {
        MaxResultCount = TestRunQueryFields.DefaultPageSize;
    }
}
