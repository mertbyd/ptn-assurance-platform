using System.Collections.Generic;

namespace Ptn.TestModule.Documents.Bridge.Profiles;

// islevi: YAML yol tetikleyicisinin status ve operasyon alanlarini tasir.
// sistemdeki gorevi: Yol secimini kapali tetikleyici listeleriyle sinirlar.
internal sealed class EvidenceTriggerDocument
{
    public List<int> StatusCodes { get; set; } = [];
    public List<string> OperationIds { get; set; } = [];
}
