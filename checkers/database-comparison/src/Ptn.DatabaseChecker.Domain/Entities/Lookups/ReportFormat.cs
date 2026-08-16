using System;

namespace Ptn.DatabaseChecker.Entities.Lookups;

// islevi: Bir raporun formatini (Html / Markdown) referans/master veri olarak tutar (enum degil, lookup tablosu).
// sistemdeki gorevi: ComparisonReportContent (ComparisonRun.Reports, owned JSON) bu lookup'in Code'unu kararli olarak tasir - FK degil; HTML mail'e gomulur, Markdown wiki/PR yorumuna yapistirilir. Ortak alanlar LookupEntity'den gelir.
public class ReportFormat : LookupEntity
{
    // EF Core materializasyonu icin parametresiz ctor; disaridan cagrilmaz.
    protected ReportFormat()
    {
    }

    // Seed ve CRUD tarafinin Id'yi disaridan verip satiri kurdugu ctor; ortak alanlar base'e devredilir.
    public ReportFormat(Guid id, string code, string name, string? description = null, bool isActive = true)
        : base(id, code, name, description, isActive)
    {
    }
}
