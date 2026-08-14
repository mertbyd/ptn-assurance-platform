using System.Collections.Generic;

namespace Ptn.TestModule.Models.Runs;

// islevi: Belgedeki tek bir is akisinin kimligini ve adimlarini tasir.
// sistemdeki gorevi: Adim kimliklerinin kaynak sirasini domain tarafinda korur.
/// <summary>
/// Kosum belgesindeki bir is akisini tasir.
/// </summary>
public class WorkflowDocumentWorkflow
{
    /// <summary>Is akisinin belge icindeki kimligidir.</summary>
    public string? WorkflowId { get; set; }

    /// <summary>Is akisinin adimlaridir.</summary>
    public IReadOnlyList<WorkflowDocumentStep> Steps { get; set; } = [];
}
