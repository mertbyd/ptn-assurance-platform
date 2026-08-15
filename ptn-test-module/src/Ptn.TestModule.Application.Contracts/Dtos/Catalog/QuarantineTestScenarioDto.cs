using System;

namespace Ptn.TestModule.Dtos.Catalog;

// islevi: Senaryoyu karantinaya alma isteginin son kullanma tarihini ve gerekcesini tasir.
// sistemdeki gorevi: Suresiz karantinayi sozlesme seviyesinde imkansiz kilar; sure zorunludur (PLAN-0003 TM-28 §2.5).
/// <summary>
/// Bir senaryoyu sinirli sureyle karantinaya alma istegidir.
/// </summary>
public class QuarantineTestScenarioDto
{
    /// <summary>Karantinanin bittigi andir; zorunludur ve gelecege donuk olmalidir.</summary>
    public DateTime? QuarantineUntil { get; set; }

    /// <summary>Karantinaya alma gerekcesidir.</summary>
    public string? QuarantineReason { get; set; }
}
