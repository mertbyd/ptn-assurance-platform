namespace Ptn.TestModule.Constants.Runs;

// islevi: Dis Arazzo runner cagrisinin pinli imaj, surum hedefi, dosya adi ve butce sozlesmesini tanimlar.
// sistemdeki gorevi: Planner, surec siniri ve testlerin ayni runner sozlugunu kullanmasini saglar (ADR-0015 §A).
/// <summary>
/// Redocly Respect kosum sinirinin kararli surum, dosya ve zaman asimi sabitlerini tasir.
/// </summary>
public static class WorkflowRunnerConsts
{
    /// <summary>Konteyner surecini baslatan executable adidir.</summary>
    public const string DockerExecutable = "docker";

    /// <summary>Kosum davranisini kararli tutan pinli Redocly CLI imajidir.</summary>
    public const string RedoclyCliImage = "redocly/cli:2.14.0";

    /// <summary>Belgelerin dogrulandigi tek desteklenen Arazzo surumudur (AUDIT-0002 BULGU-07).</summary>
    public const string ArazzoTargetVersion = "1.0.1";

    /// <summary>test_runs.runner_ref alanina yazilan runner kimlik bicimidir.</summary>
    public const string RunnerRefFormat = "respect@{0}";

    /// <summary>Girdilerin surece process listesine dusmeden gectigi ortam degiskenidir (AUDIT-0002 BULGU-09).</summary>
    public const string InputEnvironmentVariableName = "REDOCLY_CLI_RESPECT_INPUT";

    /// <summary>Kosum belgesinin konteynere baglanan dosya adidir.</summary>
    public const string RunDocumentFileName = "workflow.arazzo.yaml";

    /// <summary>Runner'in urettigi HAR 1.2 artefakt dosya adidir.</summary>
    public const string HarOutputFileName = "run.har";

    /// <summary>Runner'in urettigi JSON ozet dosya adidir.</summary>
    public const string JsonOutputFileName = "run.json";

    /// <summary>Belgenin salt-okunur baglandigi konteyner ici klasordur.</summary>
    public const string DocumentMountTarget = "/spec";

    /// <summary>Artefaktlarin yazildigi konteyner ici klasordur.</summary>
    public const string OutputMountTarget = "/out";

    /// <summary>Tum senaryonun runner tarafindaki varsayilan azami suresidir.</summary>
    public const int DefaultExecutionTimeoutSeconds = 3_600;

    /// <summary>Tek bir HTTP cagrisinin runner tarafindaki varsayilan azami suresidir.</summary>
    public const int DefaultMaxFetchTimeoutSeconds = 40;

    /// <summary>Runner butcesi dolduktan sonra sert kill'e kadar taninan ek suredir.</summary>
    public const int HardKillGraceMs = 60_000;

    /// <summary>Runner JSON ozetinin satir ici tutulan azami karakter sayisidir.</summary>
    public const int MaxJsonSummaryLength = 16_384;

    /// <summary>Kosum surecinin gecici calisma klasoru ad bolumudur.</summary>
    public const string WorkspaceName = "arazzo-run";

    /// <summary>Belgenin salt-okunur baglandigi calisma klasoru bolumudur.</summary>
    public const string DocumentDirectoryName = "spec";

    /// <summary>Artefaktlarin yazildigi calisma klasoru bolumudur.</summary>
    public const string OutputDirectoryName = "out";

    /// <summary>Belgenin Arazzo surumunu bildiren kok alan adidir.</summary>
    public const string ArazzoVersionField = "arazzo";

    /// <summary>Bir successCriteria nesnesinin degerlendirme turunu bildiren alan adidir.</summary>
    public const string CriterionTypeField = "type";

    /// <summary>Runner tarafindan desteklenmeyen kriter turudur.</summary>
    public const string XPathCriterionType = "xpath";

    /// <summary>Checker yanitinda adim kimliginin echo edildigi alan adidir (ADR-0021).</summary>
    public const string CorrelationStepKeyField = "stepKey";

    /// <summary>Derlenmis her Arazzo adiminin istek korelasyonunu tasiyan header adidir (ADR-0022).</summary>
    public const string StepKeyHeaderName = "X-CheckNexus-Step-Key";

    /// <summary>Checker yanitinda korelasyon nesnesinin adidir (ADR-0021).</summary>
    public const string CorrelationField = "correlation";

    /// <summary>Checker yanitinda hukum kodunun bildirildigi alan adidir.</summary>
    public const string OutcomeCodeField = "outcomeCode";

    /// <summary>Belgedeki is akisi koleksiyonunun alan adidir.</summary>
    public const string WorkflowsField = "workflows";

    /// <summary>Bir is akisindaki adim koleksiyonunun alan adidir.</summary>
    public const string StepsField = "steps";

    /// <summary>Bir adimin kararli kimligini tasiyan alan adidir.</summary>
    public const string StepIdField = "stepId";

    // islevi: Runner'a ortam degiskeniyle gecirilen runtime girdi anahtarlarini merkezilestirir.
    /// <summary>Kosum girdi sozlugunun kararli anahtarlarini tasir.</summary>
    public static class Inputs
    {
        public const string BaseUrl = "baseUrl";
        public const string EnvironmentKey = "environmentKey";
        public const string SpecSnapshotId = "specSnapshotId";
        public const string DbConnectionId = "dbConnectionId";
        public const string TraceId = "traceId";
    }
}
