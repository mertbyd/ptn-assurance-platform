using System.Collections.Generic;

namespace Ptn.TestModule.Constants.Bridge;

// islevi: Yazarlik ajaninin alti karar anini kapali sozlukte tanimlar.
// sistemdeki gorevi: Her an icin ayri tool, tur ve token butcesinin ABP Setting anahtaridir.
public static class AgentMomentCodes
{
    public const string Grounding = "Grounding";
    public const string Drafting = "Drafting";
    public const string Validation = "Validation";
    public const string Approval = "Approval";
    public const string Execution = "Execution";
    public const string Diagnosis = "Diagnosis";
    public static IReadOnlyCollection<string> All { get; } = [Grounding, Drafting, Validation, Approval, Execution, Diagnosis];
}
