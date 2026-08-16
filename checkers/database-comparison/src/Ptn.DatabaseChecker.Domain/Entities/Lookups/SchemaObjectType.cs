using System;

namespace Ptn.DatabaseChecker.Entities.Lookups;

// islevi: Bir sema farkinin nesne turunu (Table/View/Column/Index/PrimaryKey...) referans/master veri olarak tutar (enum degil, lookup tablosu).
// sistemdeki gorevi: SchemaDifferenceModel (ComparisonRun.Findings, owned JSON) bu lookup'in Code'unu kararli olarak tasir - FK degil; rapor farklari tur bazinda gruplar ("Tablolar: 2, Index'ler: 5"). Ortak alanlar LookupEntity'den gelir.
public class SchemaObjectType : LookupEntity
{
    // EF Core materializasyonu icin parametresiz ctor; disaridan cagrilmaz.
    protected SchemaObjectType()
    {
    }

    // Seed ve CRUD tarafinin Id'yi disaridan verip satiri kurdugu ctor; ortak alanlar base'e devredilir.
    public SchemaObjectType(Guid id, string code, string name, string? description = null, bool isActive = true)
        : base(id, code, name, description, isActive)
    {
    }
}
