namespace Ptn.TestModule.Constants.Runs;

// islevi: Test Module'un dokuz tablosunun kararli adlarini tanimlar.
// sistemdeki gorevi: EF configuration'lari tablo adini buradan okur; sema adi TestModuleDbProperties'ten gelir (ADR-0016 §A, RULE-0002).
public static class TestModuleTableNames
{
    // test_lookup semasi: sabit deger listeleri, seed edilir, global referans verisidir.
    public const string RunStatuses = "test_run_statuses";
    public const string OutcomeStatuses = "test_outcome_statuses";
    public const string FailureCategories = "test_failure_categories";
    public const string TriggerKinds = "test_trigger_kinds";
    public const string ScenarioStates = "test_scenario_states";

    // test_catalog semasi: tanim dunyasi.
    public const string Scenarios = "test_scenarios";

    // test_run semasi: kosum dunyasi.
    public const string Runs = "test_runs";
    public const string RunResults = "test_run_results";
    public const string ResultFindings = "test_result_findings";
}
