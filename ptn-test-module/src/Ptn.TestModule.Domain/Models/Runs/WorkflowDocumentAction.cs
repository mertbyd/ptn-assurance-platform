using System.Collections.Generic;

namespace Ptn.TestModule.Models.Runs;

// islevi: Bir adim dalinin adini ve tasidigi kriterleri tasir.
// sistemdeki gorevi: Kriter taramasinin yalniz basari dalina bakmasini engeller.
/// <summary>
/// Kosum belgesindeki bir adim aksiyonunu tasir.
/// </summary>
public class WorkflowDocumentAction
{
    /// <summary>Aksiyonun belge icindeki adidir.</summary>
    public string? Name { get; set; }

    /// <summary>Aksiyonun tetiklenme kriterleridir.</summary>
    public IReadOnlyList<WorkflowDocumentCriterion> Criteria { get; set; } = [];
}
