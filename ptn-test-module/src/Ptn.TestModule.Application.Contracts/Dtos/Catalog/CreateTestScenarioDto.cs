namespace Ptn.TestModule.Dtos.Catalog;

// islevi: Yeni senaryo surumunun public olusturma alanlarini tasir.
// sistemdeki gorevi: Version ve durum kararlarini Manager'a birakip yalniz yazarlik girdisini kabul eder.
public sealed class CreateTestScenarioDto
{
    public string ScenarioKey { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string SourceDocument { get; set; } = string.Empty;
    public string? SourceHash { get; set; }
    public TestScenarioMaterialSealDto MaterialSeal { get; set; } = new();
    public string? DerivabilityCode { get; set; }
    public bool AuthoredByAgent { get; set; }
    public string? AgentModelRef { get; set; }
    public string? Notes { get; set; }
}
