using System;
using Nexum.Abp.Foundation.Lookups;

namespace Ptn.TestModule.Entities.Lookups;

// islevi: Kosum motorunun yasam dongusu durumlarini kalici satirlar olarak siniflandirir.
// sistemdeki gorevi: test_runs.run_status_id FK'sini kararli kodlara baglar; motor durumu hukum degildir (ADR-0016 §F).
public class TestRunStatus : LookupEntity<Guid>
{
    // EF Core materializasyonu icin parametresiz ctor.
    protected TestRunStatus()
    {
    }

    // Ortak lookup alanlarini tek taban invariantindan kurar.
    public TestRunStatus(Guid id, string code, string name, string? description = null)
        : base(id, code, name, description)
    {
    }
}
