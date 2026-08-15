using System.Collections.Generic;

namespace Ptn.TestModule.Dtos.Catalog;

// islevi: Senaryo yayin kapilarinin toplu sonucunu ve dusen kapilari tasir.
// sistemdeki gorevi: Yayin reddini bool yerine kararli gate kodlariyla public yuzeye cikarir.
public sealed class TestScenarioPublishDecisionDto
{
    public bool IsPublishable { get; set; }
    public IReadOnlyList<string> FailedGateCodes { get; set; } = [];
    public IReadOnlyList<string> Warnings { get; set; } = [];
}
