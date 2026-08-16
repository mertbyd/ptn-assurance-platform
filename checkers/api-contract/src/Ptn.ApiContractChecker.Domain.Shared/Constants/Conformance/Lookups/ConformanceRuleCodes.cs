namespace Ptn.ApiContractChecker.Constants.Conformance.Lookups;

// islevi: Response conformance oracle'inin sirali ve kapali kural kodlarini tanimlar.
// sistemdeki gorevi: Profil seviyesi, ihlal ve Test Module sonucunu ayni kural kimliklerine baglar.
public static class ConformanceRuleCodes
{
    public const string NotAServerError = "not-a-server-error";
    public const string StatusCodeConformance = "status-code-conformance";
    public const string ContentTypeConformance = "content-type-conformance";
    public const string ResponseHeadersConformance = "response-headers-conformance";
    public const string ResponseSchemaConformance = "response-schema-conformance";
    public const string AdditionalProperties = "additional-properties";
    public const string SecurityRequirement = "security-requirement";

    public static IReadOnlyCollection<string> All { get; } =
    [
        NotAServerError,
        StatusCodeConformance,
        ContentTypeConformance,
        ResponseHeadersConformance,
        ResponseSchemaConformance,
        AdditionalProperties,
        SecurityRequirement
    ];
}
