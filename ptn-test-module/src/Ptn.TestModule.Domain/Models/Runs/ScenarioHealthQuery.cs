namespace Ptn.TestModule.Models.Runs;

// islevi: Senaryo saglik sayfasinin filtre ve sayfalama girdilerini domain tarafinda tasir.
// sistemdeki gorevi: Public DTO'yu repository sorgusundan ayiran tipli okuma sozlesmesidir.
public class ScenarioHealthQuery
{
    public string? ScenarioKey { get; set; }
    public double? MinFlakyRatio { get; set; }
    public double? MaxPassRatio { get; set; }
    public string? Sorting { get; set; }
    public int SkipCount { get; set; }
    public int MaxResultCount { get; set; }
}
