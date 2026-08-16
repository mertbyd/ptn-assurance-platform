namespace Ptn.DatabaseChecker.Models.Assertions;

// islevi: Tenant-aware assertion ayarlarinin tek calisma icin cozulmus ve dogrulanmis halini tasir.
// sistemdeki gorevi: Polling cekirdegi ve saf matcher setting provider'a baglanmadan ayni limitleri kullanir.
public sealed class AssertionExecutionSettings
{
    public int MaxTimeoutMs { get; init; }
    public int MinPollIntervalMs { get; init; }
    public int MaxRowsPerAssertion { get; init; }
    public int RegexTimeoutMs { get; init; }
    public int MaxBatchSize { get; init; }
}
