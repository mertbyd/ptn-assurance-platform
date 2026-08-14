using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge;

// islevi: Tetikleyici, sirali adimlar ve kapali hukum ifadelerinden olusan kanit yolunu tasir.
// sistemdeki gorevi: Yeni teshis akisini yeni C# dali yerine profil verisi olarak eklenebilir yapar.
public sealed class EvidencePathDefinition
{
    public string PathKey { get; set; } = string.Empty;
    public EvidencePathTrigger Trigger { get; set; } = new();
    public List<EvidencePathStep> Steps { get; set; } = [];
    public string ConfirmedWhen { get; set; } = string.Empty;
    public string InconclusiveWhen { get; set; } = string.Empty;
}
