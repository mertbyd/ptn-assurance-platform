using System.Collections.Generic;

namespace Ptn.TestModule.Models.Runs;

// islevi: Kosum belgesinin domain tarafindaki surum ve is akisi yapisini tasir.
// sistemdeki gorevi: Manager'in kabul kapisi kararlarini wire tipine bagimli olmadan vermesini saglar.
/// <summary>
/// Cozumlenmis Arazzo belgesinin domain seklini tasir.
/// </summary>
public class WorkflowDocument
{
    /// <summary>Belgenin bildirdigi Arazzo surumudur.</summary>
    public string? Arazzo { get; set; }

    /// <summary>Belgedeki is akislaridir.</summary>
    public IReadOnlyList<WorkflowDocumentWorkflow> Workflows { get; set; } = [];
}
