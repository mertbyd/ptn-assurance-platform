namespace Ptn.TestModule.Dtos.Catalog;

// islevi: Salt hesap derleme ve lint sonucunu okunur bicimde tasir.
// sistemdeki gorevi: Taslagi yazmadan derlenmis belge, hash, assertion sayisi ve taniyi dondurur.
public sealed class ScenarioCompilePreviewResultDto
{
    public string CompiledDocument { get; set; } = string.Empty;
    public string CompiledHash { get; set; } = string.Empty;
    public int CompiledAssertionCount { get; set; }
    public bool IsSchemaValid { get; set; }
    public string LintDiagnostics { get; set; } = string.Empty;
}
