using System.Collections.Generic;

namespace Ptn.TestModule.Models.Runs;

// islevi: Bir adimin kararli kimligini ve kriter tasiyan tum dallarini tasir.
// sistemdeki gorevi: Adim kimligi cikarimi ile kriter taramasini tek domain seklinde birlestirir.
/// <summary>
/// Kosum belgesindeki bir is akisi adimini tasir.
/// </summary>
public class WorkflowDocumentStep
{
    /// <summary>Adimin belge icindeki kararli kimligidir.</summary>
    public string? StepId { get; set; }

    /// <summary>Adimin basari kriterleridir.</summary>
    public IReadOnlyList<WorkflowDocumentCriterion> SuccessCriteria { get; set; } = [];

    /// <summary>Adimin basari sonrasi aksiyonlaridir.</summary>
    public IReadOnlyList<WorkflowDocumentAction> OnSuccess { get; set; } = [];

    /// <summary>Adimin basarisizlik aksiyonlaridir.</summary>
    public IReadOnlyList<WorkflowDocumentAction> OnFailure { get; set; } = [];
}
