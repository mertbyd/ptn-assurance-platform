using System;
using System.Collections.Generic;
using Ptn.TestModule.Models.Bridge;

namespace Ptn.TestModule.Models.Compilation;

// islevi: Yayin kapisinin okudugu makine kanitlarini ve advisory sema uyarilarini tek sonuc nesnesinde tasir.
// sistemdeki gorevi: Kaniti istemci girdisinden tamamen ayirir; kapi yalniz derleyicinin urettigi degerlere bakar (ADR-0015 §C).
public sealed class ScenarioCompilationEvidence
{
    public string CompiledDocument { get; set; } = string.Empty;
    public string CompiledHash { get; set; } = string.Empty;
    public int AssertionCount { get; set; }
    public bool IsSchemaValid { get; set; }
    public bool AreAssertionsDerivable { get; set; }
    public List<Guid> SourceDescriptionSpecSnapshotIds { get; set; } = [];
    public string LintDiagnostics { get; set; } = string.Empty;
    public List<SchemaLintWarning> SchemaLintWarnings { get; set; } = [];
}
