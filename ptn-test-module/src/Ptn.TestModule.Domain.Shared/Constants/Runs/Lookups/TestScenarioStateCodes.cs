using System.Collections.Generic;

namespace Ptn.TestModule.Constants.Runs.Lookups;

// islevi: Senaryo surumunun yayin durumunu kararli kodlarla tanimlar.
// sistemdeki gorevi: test_scenario_states lookup'inin kapali sozlugudur; yalniz Published kosar (ADR-0016 §F, RULE-0005).
public static class TestScenarioStateCodes
{
    // Uzerinde calisiliyor; kosmaz.
    public const string Draft = "Draft";

    // Onay bekliyor; kosmaz.
    public const string PendingApproval = "PendingApproval";

    // Yayinda; kosan tek durum.
    public const string Published = "Published";

    // Kullanimdan kaldirildi; gecmis kayitlar korunur ama yeni kosum almaz.
    public const string Deprecated = "Deprecated";

    public static IReadOnlyCollection<string> All { get; } =
        [Draft, PendingApproval, Published, Deprecated];
}
