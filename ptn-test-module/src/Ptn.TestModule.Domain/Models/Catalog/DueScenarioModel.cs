using System;

namespace Ptn.TestModule.Models.Catalog;

// islevi: Vadesi gelmis zamanlanmis senaryonun kuyruklama icin gereken hafif adresini tasir.
// sistemdeki gorevi: Worker tarama sonucunu derlenmis belge gibi agir kolonlar olmadan okur (TM-22).
public class DueScenarioModel
{
    public Guid ScenarioId { get; set; }
    public string ScenarioKey { get; set; } = string.Empty;
    public string CompiledHash { get; set; } = string.Empty;

    // Vade ani tetikleyici referansina girer; boylece ayni calisma iki kez kosum uretmez.
    public DateTime? NextRunAt { get; set; }
    public Guid? TenantId { get; set; }
}
