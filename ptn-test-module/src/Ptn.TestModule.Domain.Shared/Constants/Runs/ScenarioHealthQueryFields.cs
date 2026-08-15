namespace Ptn.TestModule.Constants.Runs;

// islevi: Senaryo saglik liste sorgusunun kararli siralama alan adlarini tanimlar.
// sistemdeki gorevi: HTTP siralama token'larini Domain.Shared sahibinde tutar.
public static class ScenarioHealthQueryFields
{
    public const string ScenarioKey = nameof(ScenarioKey);
    public const string FlakyRatio = nameof(FlakyRatio);
    public const string PassRatio = nameof(PassRatio);
    public const string P95DurationMs = nameof(P95DurationMs);
    public const string TotalRunCount = nameof(TotalRunCount);
}
