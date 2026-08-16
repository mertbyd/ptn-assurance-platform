namespace Ptn.ApiContractChecker.Constants.Differences.Lookups;

// islevi: Comparison engine kapali fark katalogunun kararli tur kodlarini tanimlar.
// sistemdeki gorevi: Comparison engine bulgulari, owned JSON ve seed arasindaki kapali kod sozlesmesidir.
public static class DifferenceKindCodes
{
    public const string NewRequiredRequestProperty = "new-required-request-property";
    public const string RequestPropertyBecameRequired = "request-property-became-required";
    public const string RequestPropertyTypeChanged = "request-property-type-changed";
    public const string RequestParameterEnumValueRemoved = "request-parameter-enum-value-removed";
    public const string RequestBodyBecameRequired = "request-body-became-required";
    public const string ResponsePropertyBecameOptional = "response-property-became-optional";
    public const string ResponsePropertyBecameNullable = "response-property-became-nullable";
    public const string ResponseSuccessStatusRemoved = "response-success-status-removed";
    public const string ResponseMediaTypeRemoved = "response-media-type-removed";
    public const string RequiredResponseHeaderRemoved = "required-response-header-removed";
    public const string EndpointAdded = "endpoint-added";
    public const string EndpointRemoved = "endpoint-removed";
    public const string SchemaAdded = "schema-added";
    public const string SchemaRemoved = "api-schema-removed";
    public const string SchemaRenamed = "schema-renamed";
    public const string DescriptionChanged = "description-changed";

    public static IReadOnlyCollection<string> All { get; } =
    [
        NewRequiredRequestProperty,
        RequestPropertyBecameRequired,
        RequestPropertyTypeChanged,
        RequestParameterEnumValueRemoved,
        RequestBodyBecameRequired,
        ResponsePropertyBecameOptional,
        ResponsePropertyBecameNullable,
        ResponseSuccessStatusRemoved,
        ResponseMediaTypeRemoved,
        RequiredResponseHeaderRemoved,
        EndpointAdded,
        EndpointRemoved,
        SchemaAdded,
        SchemaRemoved,
        SchemaRenamed,
        DescriptionChanged
    ];
}
