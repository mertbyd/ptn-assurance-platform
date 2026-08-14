namespace Ptn.TestModule.Models.Compilation;

// islevi: Derlenmis Arazzo belgesini, kanonik hash'ini ve lint kanitini tasir.
// sistemdeki gorevi: source_document ile compiled_document ayrimini yayin akisina tipli sonuc olarak sunar.
public sealed class ArazzoCompilationResult
{
    public string CompiledDocument { get; set; } = string.Empty;
    public string CompiledHash { get; set; } = string.Empty;
    public int CompiledAssertionCount { get; set; }
    public bool IsSchemaValid { get; set; }
    public string LintDiagnostics { get; set; } = string.Empty;
}
