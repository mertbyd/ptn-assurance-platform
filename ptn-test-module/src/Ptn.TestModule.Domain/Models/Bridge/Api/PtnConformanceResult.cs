using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge;

// islevi: Normalize edilmis uygunluk outcome'u ile deger icermeyen ihlal adreslerini tasir.
// sistemdeki gorevi: API checker hukum sonucunu ham casing ve DTO bagimliligi olmadan domaine verir.
public sealed class PtnConformanceResult
{
    public string OutcomeCode { get; set; } = string.Empty;
    public List<PtnConformanceViolation> Violations { get; set; } = [];
}
