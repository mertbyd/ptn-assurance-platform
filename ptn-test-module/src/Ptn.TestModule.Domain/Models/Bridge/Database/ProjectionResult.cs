using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge;

// islevi: Redaksiyonlu projeksiyon satirlarini uc degerli kanit durumuyla tasir.
// sistemdeki gorevi: Eksik checker yuzeyini veya okunamayan tabloyu yanlis yokluk hukumunden ayirir.
public sealed class ProjectionResult
{
    public string StateCode { get; set; } = string.Empty;
    public List<Dictionary<string, string?>> Rows { get; set; } = [];
    public long ObservedRowCount { get; set; }
    public bool Truncated { get; set; }
    public CorrelationRef? Correlation { get; set; }
}
