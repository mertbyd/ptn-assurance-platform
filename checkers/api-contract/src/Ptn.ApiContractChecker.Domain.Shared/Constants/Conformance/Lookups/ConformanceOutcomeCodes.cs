namespace Ptn.ApiContractChecker.Constants.Conformance.Lookups;

// islevi: Response conformance oracle'inin kapali sonuc kodlarini tanimlar.
// sistemdeki gorevi: Test Module, domain sonucu ve HTTP kontratini metinden bagimsiz tek katalogda bulusturur.
public static class ConformanceOutcomeCodes
{
    public const string Passed = "passed";
    public const string StatusCodeUndocumented = "status-code-undocumented";
    public const string MediaTypeUndocumented = "media-type-undocumented";
    public const string ResponseSchemaViolation = "response-schema-violation";
    public const string RequiredHeaderMissing = "required-header-missing";
    public const string UndocumentedProperty = "undocumented-property";
    public const string ServerError = "server-error";
    public const string OperationNotResolved = "operation-not-resolved";
    public const string SnapshotNotFound = "snapshot-not-found";
    public const string PolicySuppressed = "policy-suppressed";
    public const string SchemaNotResolved = "schema-not-resolved";

    public static IReadOnlyCollection<string> All { get; } =
    [
        Passed,
        StatusCodeUndocumented,
        MediaTypeUndocumented,
        ResponseSchemaViolation,
        RequiredHeaderMissing,
        UndocumentedProperty,
        ServerError,
        OperationNotResolved,
        SnapshotNotFound,
        PolicySuppressed,
        SchemaNotResolved
    ];
}
