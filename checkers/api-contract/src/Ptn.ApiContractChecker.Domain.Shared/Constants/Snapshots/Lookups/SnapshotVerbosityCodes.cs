namespace Ptn.ApiContractChecker.Constants.Snapshots.Lookups;

// islevi: Snapshot yazarlik cevaplarinin desteklenen ayrinti seviyelerini tanimlar.
// sistemdeki gorevi: Minimal varsayilanini ve acik normal/full taleplerini kapali kod kumesinde tutar.
public static class SnapshotVerbosityCodes
{
    public const string Minimal = "minimal";
    public const string Normal = "normal";
    public const string Full = "full";
    public static readonly IReadOnlyCollection<string> All = [Minimal, Normal, Full];
}
