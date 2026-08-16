using System;

namespace Ptn.DatabaseChecker.Entities.Lookups;

// islevi: Bir kapsam kuralinin anlamini (Include / Exclude / DataCompare) referans/master veri olarak tutar (enum degil, lookup tablosu).
// sistemdeki gorevi: Kapsam kurallari (ComparisonDefinition.ScopeRules ve ComparisonRun.ScopeSnapshot, owned JSON) bu lookup'in Code'unu kararli olarak tasir - FK degil, donmus kod; UI listesi ve kod gecerliligi bu tablodan beslenir. Ortak alanlar LookupEntity'den gelir.
public class ScopeKind : LookupEntity
{
    // EF Core materializasyonu icin parametresiz ctor; disaridan cagrilmaz.
    protected ScopeKind()
    {
    }

    // Seed ve CRUD tarafinin Id'yi disaridan verip satiri kurdugu ctor; ortak alanlar base'e devredilir.
    public ScopeKind(Guid id, string code, string name, string? description = null, bool isActive = true)
        : base(id, code, name, description, isActive)
    {
    }
}
