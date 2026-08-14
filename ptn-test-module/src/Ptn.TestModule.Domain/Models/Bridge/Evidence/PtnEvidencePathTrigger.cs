using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge;

// islevi: Kanit yolunun kapali HTTP ve operasyon tetikleyicilerini tasir.
// sistemdeki gorevi: Yol secimini serbest ifade veya hard-coded vaka dalindan uzak tutar.
public sealed class PtnEvidencePathTrigger
{
    public List<int> StatusCodes { get; set; } = [];
    public List<string> OperationIds { get; set; } = [];
}
