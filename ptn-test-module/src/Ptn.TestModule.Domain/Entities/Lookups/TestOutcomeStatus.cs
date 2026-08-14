using System;
using Nexum.Abp.Foundation.Lookups;

namespace Ptn.TestModule.Entities.Lookups;

// islevi: Tek bir testin hukmunu ve bu hukmun build'i kirip kirmadigini kalici satirlar olarak siniflandirir.
// sistemdeki gorevi: test_run_results.outcome_status_id FK'sini kararli kodlara baglar ve build politikasini koddan cikarir (ADR-0016 §F).
public class TestOutcomeStatus : LookupEntity<Guid>
{
    // Bu hukmun build'i kirip kirmadigi; politika veride tutulur, boylece yeni bir hukum eklendiginde kod dallanmasi bozulmaz.
    public bool BreaksBuild { get; internal set; }

    // EF Core materializasyonu icin parametresiz ctor.
    protected TestOutcomeStatus()
    {
    }

    // Ortak lookup alanlarini taban invariantindan, build politikasini ise kendi alanindan kurar.
    public TestOutcomeStatus(Guid id, string code, string name, bool breaksBuild, string? description = null)
        : base(id, code, name, description)
    {
        BreaksBuild = breaksBuild;
    }
}
