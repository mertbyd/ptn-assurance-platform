using System.Collections.Generic;

namespace Ptn.TestModule.Models.Catalog;

// islevi: Senaryo yayin kapilarinin toplu kararini ve dusen kapilari tasir.
// sistemdeki gorevi: Published gecisinin bool yerine hangi makine kapilarinda durdugunu kararli kodlarla raporlamasini saglar.
public sealed class TestScenarioPublishDecision
{
    public bool IsPublishable { get; set; }
    public IReadOnlyList<string> FailedGateCodes { get; set; } = [];
    public IReadOnlyList<string> Warnings { get; set; } = [];
}
