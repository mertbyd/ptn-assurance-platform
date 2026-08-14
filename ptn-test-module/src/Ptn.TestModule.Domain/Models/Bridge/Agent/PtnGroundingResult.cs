using System.Collections.Generic;
using Ptn.TestModule.Models.Bridge;
using Ptn.TestModule.Models.Bridge.Footprint;

namespace Ptn.TestModule.Models.Bridge.Agent;

// islevi: Operasyon zemini, istek ornegi, tablo bilgisi, footprint ve kapsami tek cevapta tasir.
// sistemdeki gorevi: ptn_ground ara sonuclarinin ajan baglamina ayri tool cagrilari olarak sizmasini engeller.
public sealed class PtnGroundingResult
{
    public string ResponseFormat { get; set; } = string.Empty;
    public PtnCoverageReport Coverage { get; set; } = new();
    public string DecisionCode { get; set; } = string.Empty;
    public string CriticalFactCode { get; set; } = string.Empty;
    public PtnOperationBinding? OperationBinding { get; set; }
    public PtnRequestExample? RequestExample { get; set; }
    public PtnTableDescription? TableDescription { get; set; }
    public PtnFootprintResult Footprint { get; set; } = new();
    public List<PtnClosedQuestion> Questions { get; set; } = [];
    public string? ResourceLink { get; set; }
}
