namespace Ptn.ApiContractChecker.Constants.Runs.Lookups;

// islevi: Run durum lookup satirlarinin varsayilan gorunen adlarini tanimlar.
// sistemdeki gorevi: Seed metinlerinin kod icinde dagilmasini engeller.
public static class CheckRunStatusNames
{
    public const string Pending = "Pending";
    public const string Running = "Running";
    public const string Completed = "Completed";
    public const string Failed = "Failed";
    public const string Partial = "Partial";
}
