using System;
using Nexum.Abp.Foundation.Lookups;

namespace Ptn.TestModule.Entities.Lookups;

// islevi: Bir kosunun nasil baslatildigini kalici satirlar olarak siniflandirir.
// sistemdeki gorevi: test_runs.trigger_kind_id FK'sini kararli kodlara baglar; "kim baslatti" CreatorId'dir (ADR-0016 §E).
public class TestTriggerKind : LookupEntity<Guid>
{
    // EF Core materializasyonu icin parametresiz ctor.
    protected TestTriggerKind()
    {
    }

    // Ortak lookup alanlarini tek taban invariantindan kurar.
    public TestTriggerKind(Guid id, string code, string name, string? description = null)
        : base(id, code, name, description)
    {
    }
}
