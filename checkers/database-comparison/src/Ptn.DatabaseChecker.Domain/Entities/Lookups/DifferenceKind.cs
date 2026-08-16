using System;

namespace Ptn.DatabaseChecker.Entities.Lookups;

// islevi: Bir farkin yonunu (OnlyInSource / OnlyInTarget / Modified) referans/master veri olarak tutar (enum degil, lookup tablosu).
// sistemdeki gorevi: Schema/Migration/DataRow bulgu modelleri (ComparisonRun.Findings, owned JSON) bu lookup'in Code'unu ORTAK ve kararli olarak tasir - FK degil; "sadece kaynakta / sadece hedefte / degismis" yorumu bu koddan cozulur. Ortak alanlar LookupEntity'den gelir.
public class DifferenceKind : LookupEntity
{
    // EF Core materializasyonu icin parametresiz ctor; disaridan cagrilmaz.
    protected DifferenceKind()
    {
    }

    // Seed ve CRUD tarafinin Id'yi disaridan verip satiri kurdugu ctor; ortak alanlar base'e devredilir.
    public DifferenceKind(Guid id, string code, string name, string? description = null, bool isActive = true)
        : base(id, code, name, description, isActive)
    {
    }
}
