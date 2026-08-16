using System;
using System.Collections.Generic;
using Ptn.TestModule.Models.Bridge;
using Ptn.TestModule.Models.Bridge.Footprint;

namespace Ptn.TestModule.Models.Bridge.Agent;

// islevi: Operasyon zemini, istek ornegi, tablo bilgisi, footprint ve kapsami tek cevapta tasir.
// sistemdeki gorevi: ptn_ground ara sonuclarinin ajan baglamina ayri tool cagrilari olarak sizmasini engeller.
public sealed class GroundingResult
{
    public string ResponseFormat { get; set; } = string.Empty;
    public CoverageReport Coverage { get; set; } = new();
    public string DecisionCode { get; set; } = string.Empty;
    public string CriticalFactCode { get; set; } = string.Empty;
    public OperationBinding? OperationBinding { get; set; }
    public RequestExample? RequestExample { get; set; }
    public TableDescription? TableDescription { get; set; }
    public FootprintResult Footprint { get; set; } = new();
    public List<ClosedQuestion> Questions { get; set; } = [];
    public string? ResourceLink { get; set; }
    public Guid? SessionId { get; set; }
    public int StepCount { get; set; }
    public List<string> PendingQuestionCodes { get; set; } = [];
}
