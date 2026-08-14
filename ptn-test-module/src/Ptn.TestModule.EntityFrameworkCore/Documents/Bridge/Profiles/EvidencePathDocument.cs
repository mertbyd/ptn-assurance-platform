using System.Collections.Generic;

namespace Ptn.TestModule.Documents.Bridge.Profiles;

// islevi: YAML icindeki tek kanit yolunun transport alanlarini tasir.
// sistemdeki gorevi: Yol verisini serbest nesne yerine bilinen semayla sinirlar.
internal sealed class EvidencePathDocument
{
    public string PathKey { get; set; } = string.Empty;
    public EvidenceTriggerDocument Trigger { get; set; } = new();
    public List<EvidenceStepDocument> Steps { get; set; } = [];
    public string ConfirmedWhen { get; set; } = string.Empty;
    public string InconclusiveWhen { get; set; } = string.Empty;
}
