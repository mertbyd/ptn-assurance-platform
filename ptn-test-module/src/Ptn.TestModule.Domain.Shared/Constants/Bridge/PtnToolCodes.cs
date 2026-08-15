using System.Collections.Generic;

namespace Ptn.TestModule.Constants.Bridge;

// islevi: Ajanin gorebildigi Bridge tool kodlarini ve aktif butceyi kapali bir kumede tanimlar.
// sistemdeki gorevi: Tool sayisinin yedi sinirini asmasini ve kademe-4 eylemlerinin kataloga sizmasini engeller.
public static class PtnToolCodes
{
    public const string Ground = "ptn_ground";
    public const string Validate = "ptn_validate";
    public const string Run = "ptn_run";
    public const string Result = "ptn_result";
    public const string Explain = "ptn_explain";
    public const string Knowledge = "ptn_knowledge";
    public const string Impact = "ptn_impact";
    public const string DryRun = "ptn_dry_run";
    public const string Task = "ptn_task";
    public const string Profile = "ptn_profile";
    public const string PatchSuggest = "ptn_patch_suggest";
    public const string PatchReview = "ptn_patch_review";
    public const string AuthoringToolset = "authoring";
    public const string RuntimeToolset = "runtime";
    public const int ActiveMax = 7;
    public const int ProtocolMax = 12;

    public static IReadOnlyCollection<string> Active { get; } = [Ground, Explain, Validate, Knowledge];
    public static IReadOnlyCollection<string> All { get; } =
        [Ground, Validate, Run, Result, Explain, Knowledge, Impact, DryRun, Task, Profile];
    public static IReadOnlyCollection<string> ReviewOnly { get; } = [PatchSuggest, PatchReview];
    public static IReadOnlyCollection<string> Governed { get; } =
        [Ground, Validate, Run, Result, Explain, Knowledge, Impact, DryRun, Task, Profile, PatchSuggest, PatchReview];
}
