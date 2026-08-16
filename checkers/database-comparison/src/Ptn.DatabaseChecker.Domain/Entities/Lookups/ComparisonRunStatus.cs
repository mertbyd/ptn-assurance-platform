using System;

namespace Ptn.DatabaseChecker.Entities.Lookups;

// islevi: Bir karsilastirma calistirmasinin yasam dongusu durumunu (Pending/Running/Completed/Failed) referans/master veri olarak tutar (enum degil, lookup tablosu).
// sistemdeki gorevi: ComparisonRun bu tabloya FK ile baglanir; asenkron tetikle-sonra-durum-sorgula akisinin durumu satir olarak tutulur. Ortak alanlar LookupEntity'den gelir.
public class ComparisonRunStatus : LookupEntity
{
    // EF Core materializasyonu icin parametresiz ctor; disaridan cagrilmaz.
    protected ComparisonRunStatus()
    {
    }

    // Seed ve CRUD tarafinin Id'yi disaridan verip satiri kurdugu ctor; ortak alanlar base'e devredilir.
    public ComparisonRunStatus(Guid id, string code, string name, string? description = null, bool isActive = true)
        : base(id, code, name, description, isActive)
    {
    }
}
