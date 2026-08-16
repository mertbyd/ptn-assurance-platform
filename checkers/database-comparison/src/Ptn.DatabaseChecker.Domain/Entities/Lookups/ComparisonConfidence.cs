using System;

namespace Ptn.DatabaseChecker.Entities.Lookups;

// islevi: Bir sema farkina duyulan guveni (Exact/Canonical/Approximate/Incomparable) referans/master veri olarak tutar (enum degil, lookup tablosu).
// sistemdeki gorevi: SchemaDifferenceModel (ComparisonRun.Findings, owned JSON) bu lookup'in Code'unu kararli olarak tasir - FK degil; ayni motor -> Exact, capraz motor -> Canonical/Approximate, karsiligi olmayan -> Incomparable ("makine kiyaslayamaz, insan baksin"). Ortak alanlar LookupEntity'den gelir.
public class ComparisonConfidence : LookupEntity
{
    // EF Core materializasyonu icin parametresiz ctor; disaridan cagrilmaz.
    protected ComparisonConfidence()
    {
    }

    // Seed ve CRUD tarafinin Id'yi disaridan verip satiri kurdugu ctor; ortak alanlar base'e devredilir.
    public ComparisonConfidence(Guid id, string code, string name, string? description = null, bool isActive = true)
        : base(id, code, name, description, isActive)
    {
    }
}
