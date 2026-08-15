using System;
using Nexum.Abp.Foundation.Lookups;

namespace Ptn.TestModule.Entities.Lookups;

// islevi: Bir bulgunun hangi hakemden geldigini kalici satirlar olarak siniflandirir.
// sistemdeki gorevi: Bulgu kaynagini checker modul adindan bagimsiz, kararli kodlara baglar (ADR-0016 §F).
public class TestFailureCategory : LookupEntity<Guid>
{
    // EF Core materializasyonu icin parametresiz ctor.
    protected TestFailureCategory()
    {
    }

    // Ortak lookup alanlarini tek taban invariantindan kurar.
    public TestFailureCategory(Guid id, string code, string name, string? description = null)
        : base(id, code, name, description)
    {
    }
}
