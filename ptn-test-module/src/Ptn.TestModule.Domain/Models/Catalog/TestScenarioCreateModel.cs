namespace Ptn.TestModule.Models.Catalog;

// islevi: Yeni senaryo surumunun normalize edilmeden onceki domain girdisini tasir.
// sistemdeki gorevi: AppService DTO'sunu Manager'in surum, benzersizlik ve Draft kurallarindan ayirir.
public sealed class TestScenarioCreateModel
{
    public string ScenarioKey { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string SourceDocument { get; set; } = string.Empty;
    public string? SourceHash { get; set; }
    public TestScenarioMaterialSeal MaterialSeal { get; set; } = new();
    public string? DerivabilityCode { get; set; }
    public bool AuthoredByAgent { get; set; }
    public string? AgentModelRef { get; set; }
    public string? Notes { get; set; }
}
