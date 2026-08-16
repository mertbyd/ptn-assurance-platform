using System;

namespace Ptn.DatabaseChecker.Entities.Lookups;

// islevi: Bir karsilastirmanin modunu (SchemaOnly / DataOnly / Both) referans/master veri olarak tutar (enum degil, lookup tablosu).
// sistemdeki gorevi: ComparisonDefinition ve ComparisonRun bu tabloya FK ile baglanir; "yapiya mi, veriye mi, ikisine mi bakilacak" karari satir olarak tutulur. Ortak alanlar LookupEntity'den gelir.
public class ComparisonType : LookupEntity
{
    // EF Core materializasyonu icin parametresiz ctor; disaridan cagrilmaz.
    protected ComparisonType()
    {
    }

    // Seed ve CRUD tarafinin Id'yi disaridan verip satiri kurdugu ctor; ortak alanlar base'e devredilir.
    public ComparisonType(Guid id, string code, string name, string? description = null, bool isActive = true)
        : base(id, code, name, description, isActive)
    {
    }
}
