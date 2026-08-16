namespace Ptn.ApiContractChecker.Constants.Runs.Lookups;

// islevi: Bir bulgunun onceki tamamlanmis kosuya gore kararli degisim durumlarini tanimlar.
// sistemdeki gorevi: Bakim ani filtreleriyle istemci yorumunun ayni kapali kod kumesini kullanmasini saglar.
public static class FindingChangeStateCodes
{
    public const string New = "New";
    public const string Known = "Known";
    public const string Resolved = "Resolved";
    public const string Unknown = "Unknown";

    public static readonly IReadOnlyCollection<string> All = [New, Known, Resolved, Unknown];
}
