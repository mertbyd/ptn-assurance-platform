using System.Collections.Generic;

namespace Ptn.TestModule.Constants.Compilation;

// islevi: Arazzo derleme, CheckNexus DB uzantisi ve Redocly lint sozlesmesinin kararli degerlerini tanimlar.
// sistemdeki gorevi: Derleyici, surec siniri ve testlerin ayni surum, alan ve butce sozlugunu kullanmasini saglar.
public static class ArazzoCompilationConsts
{
    public const string TargetVersion = "1.0.1";
    public const string DatabaseExtension = "x-checknexus-db";
    public const string DatabaseSourceDescriptionName = "databaseChecker";
    public const string DatabaseConnectionRuntimeExpression = "{$inputs.dbConnectionId}";
    public const string PassedCriterion = "$response.body#/data/passed == true";
    public const string ObservedRowCountExpression = "$response.body#/data/observedRowCount";
    public const string DockerExecutable = "docker";
    public const string RedoclyCliImage = "redocly/cli:2.14.0";
    public const string LintDocumentFileName = "document.arazzo.yaml";
    public const string LintWorkspaceName = "arazzo-lint";
    public const int DefaultTimeoutMs = 5_000;
    public const int DefaultPollIntervalMs = 250;
    public const int LintTimeoutMs = 60_000;
    public const int MaxDocumentBytes = 1_048_576;
    public const int MaxLintDiagnosticsLength = 16_384;

    // islevi: Desteklenen DB assertion operasyonlarini uzanti sozlugunde kapali tutar.
    public static class Operations
    {
        public const string AssertRow = "assertRow";
        public const string AssertCount = "assertCount";
        public const string AssertAbsent = "assertAbsent";

        public static IReadOnlyCollection<string> All { get; } =
            [AssertRow, AssertCount, AssertAbsent];
    }

    // islevi: Derleyicinin okudugu ve urettigi Arazzo/YAML alan adlarini merkezilestirir.
    public static class Fields
    {
        public const string Arazzo = "arazzo";
        public const string SourceDescriptions = "sourceDescriptions";
        public const string Name = "name";
        public const string Workflows = "workflows";
        public const string Steps = "steps";
        public const string StepId = "stepId";
        public const string Operation = "operation";
        public const string Concept = "concept";
        public const string Key = "key";
        public const string Expect = "expect";
        public const string Matcher = "matcher";
        public const string Value = "value";
        public const string Values = "values";
        public const string Tolerance = "tolerance";
        public const string Cardinality = "cardinality";
        public const string Kind = "kind";
        public const string Count = "count";
        public const string Timeout = "timeout";
        public const string TimeoutMs = "timeoutMs";
        public const string PollIntervalMs = "pollIntervalMs";
        public const string IncludeRowOnFailure = "includeRowOnFailure";
        public const string OperationId = "operationId";
        public const string OperationPath = "operationPath";
        public const string WorkflowId = "workflowId";
        public const string RequestBody = "requestBody";
        public const string ContentType = "contentType";
        public const string Payload = "payload";
        public const string ConnectionId = "connectionId";
        public const string SchemaName = "schemaName";
        public const string TableName = "tableName";
        public const string KeyValues = "keyValues";
        public const string Expectations = "expectations";
        public const string ColumnName = "columnName";
        public const string MatcherKindCode = "matcherKindCode";
        public const string ExpectedValue = "expectedValue";
        public const string ExpectedValues = "expectedValues";
        public const string KindCode = "kindCode";
        public const string ExpectedCount = "expectedCount";
        public const string SuccessCriteria = "successCriteria";
        public const string Condition = "condition";
        public const string Type = "type";
        public const string Outputs = "outputs";
        public const string ObservedRowCount = "observedRowCount";
    }
}
