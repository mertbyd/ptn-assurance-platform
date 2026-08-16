namespace Ptn.ApiContractChecker.Constants.Runs.Lookups;

// islevi: Kontrol calistirmasi yasam dongusunun kararli durum kodlarini tanimlar.
// sistemdeki gorevi: Run gecisleri, seed ve sorgular arasindaki derleme zamani koprusudur.
public static class CheckRunStatusCodes
{
    public const string Pending = "pending";
    public const string Running = "running";
    public const string Completed = "completed";
    public const string Failed = "failed";
    public const string Partial = "partial";

    public static IReadOnlyCollection<string> All { get; } =
        [Pending, Running, Completed, Failed, Partial];
}
