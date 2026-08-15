using System;
using Ptn.TestModule.Constants.Runs;
using Volo.Abp.Application.Dtos;

namespace Ptn.TestModule.Dtos.Runs;

// islevi: Kosum listesi sayfalama girdisini tanimlar.
// sistemdeki gorevi: Liste ucunun tipli ve sinirli paging sozlesmesidir; agir rapor kolonlarini tasimaz.
public sealed class TestRunListInput : PagedResultRequestDto
{
    public string? RunStatusCode { get; set; }
    public string? EnvironmentKey { get; set; }
    public Guid? ScenarioId { get; set; }
    public string? TriggerKindCode { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public bool? IsDryRun { get; set; }
    public string? Sorting { get; set; }

    public TestRunListInput()
    {
        MaxResultCount = TestRunQueryFields.DefaultPageSize;
    }
}
