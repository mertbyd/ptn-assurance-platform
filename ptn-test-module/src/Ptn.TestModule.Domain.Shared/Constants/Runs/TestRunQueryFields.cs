namespace Ptn.TestModule.Constants.Runs;

// islevi: Kosum ve bulgu liste sorgularinin kararli alan adlarini tanimlar.
// sistemdeki gorevi: HTTP siralama ve filtre token'larini Domain.Shared sahibinde tutar.
public static class TestRunQueryFields
{
    public const string CreationTime = nameof(CreationTime);
    public const string StartedAt = nameof(StartedAt);
    public const string CompletedAt = nameof(CompletedAt);
    public const string DurationMs = nameof(DurationMs);
    public const string TestKey = nameof(TestKey);
    public const string EnvironmentKey = nameof(EnvironmentKey);
    public const string Attempt = nameof(Attempt);
    public const int DefaultPageSize = 20;
    public const int MaxFingerprintCount = 100;
}
