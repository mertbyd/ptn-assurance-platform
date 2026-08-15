namespace Ptn.TestModule.ExceptionCodes.Runs;

// islevi: Lookup kod cakismalarinin kararli hata kodlarini tanimlar.
// sistemdeki gorevi: Foundation'in genel LookupCodeAlreadyExists kodunu lookup basina ayristirir; cagiran hangi sozlukte cakisma oldugunu mesaj metnine bakmadan anlar.
public static class TestModuleLookupErrorCodes
{
    private const string Prefix = "TestModule.Lookup";

    public const string RunStatusCodeAlreadyExists = $"{Prefix}:RunStatusCodeAlreadyExists";
    public const string OutcomeStatusCodeAlreadyExists = $"{Prefix}:OutcomeStatusCodeAlreadyExists";
    public const string FailureCategoryCodeAlreadyExists = $"{Prefix}:FailureCategoryCodeAlreadyExists";
    public const string TriggerKindCodeAlreadyExists = $"{Prefix}:TriggerKindCodeAlreadyExists";
    public const string ScenarioStateCodeAlreadyExists = $"{Prefix}:ScenarioStateCodeAlreadyExists";
}
