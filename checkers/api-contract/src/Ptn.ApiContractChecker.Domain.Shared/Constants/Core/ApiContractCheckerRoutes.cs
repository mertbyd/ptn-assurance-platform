namespace Ptn.ApiContractChecker.Constants;

// islevi: Uygulama HTTP rotalarinin kararli string sozlesmesini tanimlar.
// sistemdeki gorevi: Controller route degerlerinin kod icinde dagilmasini ve sessizce ayrismasini engeller.
public static class ApiContractCheckerRoutes
{
    public const string Separator = "/";
    public const char SeparatorCharacter = '/';
    public const string ApiPrefix = "/api";
    public const string EntityById = "{id}";
    public const string LookupPassivate = "{id}/passivate";
    public const string Sources = "api/sources";
    public const string SourcePassivate = "{id}/passivate";
    public const string SourceReachabilityTest = "{id}/test";
    public const string SourceDocumentSnapshot = "{id}/documents/{documentId}/snapshot";
    public const string SourceDocumentMonitoring = "{id}/documents/{documentId}/monitoring";
    public const string Snapshots = "api/snapshots";
    public const string SnapshotOperations = "{id}/operations";
    public const string SnapshotOperationFind = "{id}/operations/find";
    public const string SnapshotSchemaDescribe = "{id}/schemas/describe";
    public const string SnapshotAuthoringResult = "authoring-results/{resultRef}";
    public const string SourceDocumentSnapshots = "~/api/sources/{id}/documents/{documentId}/snapshots";
    public const string Checks = "api/checks";
    public const string CheckStatus = "{id}/status";
    public const string CheckReport = "{id}/report";
    public const string CheckFindings = "{id}/findings";
    public const string Conformance = "api/contract-checks/conformance";
    public const string ConformanceResponse = "response";
    public const string ConformanceRequest = "request";
    public const string ConformanceRequestExample = "request-example";
    public const string ConformanceOperationBindings = "operation-bindings";
    public const string ConformanceAssertionDerivability = "assertion-derivability";
    public const string ConformanceSampleSets = "sample-sets";
    public const string ConformanceOperationLinks = "operation-links";
    public const string Diagnosis = "api/contract-checks/diagnosis";
    public const string SwaggerRoot = "~/swagger";
    public const string SwaggerDocumentEndpoint = "/swagger/v1/swagger.json";

    // Lookup endpointlerinin kararli route sozlesmesini toplar.
    public static class Lookups
    {
        public const string SpecFormats = "api/lookups/spec-formats";
        public const string CheckRunStatuses = "api/lookups/check-run-statuses";
        public const string DifferenceSeverities = "api/lookups/difference-severities";
        public const string DifferenceDirections = "api/lookups/difference-directions";
        public const string DifferenceKinds = "api/lookups/difference-kinds";
    }
}
