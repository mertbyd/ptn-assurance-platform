using System;
using Nexum.Abp.Foundation.Lookups;

namespace Ptn.TestModule.Entities.Lookups;

// islevi: Senaryo surumunun yayin durumunu kalici satirlar olarak siniflandirir.
// sistemdeki gorevi: test_scenarios.state_id FK'sini kararli kodlara baglar; yalniz Published kosar (ADR-0016 §F, RULE-0005).
public class TestScenarioState : LookupEntity<Guid>
{
    // EF Core materializasyonu icin parametresiz ctor.
    protected TestScenarioState()
    {
    }

    // Ortak lookup alanlarini tek taban invariantindan kurar.
    public TestScenarioState(Guid id, string code, string name, string? description = null)
        : base(id, code, name, description)
    {
    }
}
