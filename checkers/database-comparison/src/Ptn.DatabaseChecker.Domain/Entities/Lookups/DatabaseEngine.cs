using System;

namespace Ptn.DatabaseChecker.Entities.Lookups;

// islevi: Sistemin sema okuyabildigi veritabani motorlarini referans/master veri olarak tutar (enum degil, lookup tablosu).
// sistemdeki gorevi: UI'da "kaynak/hedef motor" listesi ve ileride karsilastirma kayitlarindan FK ile referans verilecek satirlar; Code alani okuyucu siniflariyla (DatabaseEngineCodes) eslesir. Ortak alanlar LookupEntity'den gelir.
public class DatabaseEngine : LookupEntity
{
    // EF Core materializasyonu icin parametresiz ctor; disaridan cagrilmaz.
    protected DatabaseEngine()
    {
    }

    // Seed ve CRUD tarafinin kimligi (Id) disaridan verip satiri kurdugu ctor; ortak alanlar base'e devredilir.
    public DatabaseEngine(Guid id, string code, string name, string? description = null, bool isActive = true)
        : base(id, code, name, description, isActive)
    {
    }
}
