namespace Ptn.ApiContractChecker.Constants.Differences.Lookups;

// islevi: Comparison engine fark katalogunun varsayilan gorunen adlarini tanimlar.
// sistemdeki gorevi: DifferenceKind seed metinlerini Domain.Shared altinda tek kaynaga baglar.
public static class DifferenceKindNames
{
    public const string NewRequiredRequestProperty = "New required request property";
    public const string RequestPropertyBecameRequired = "Request property became required";
    public const string RequestPropertyTypeChanged = "Request property type changed";
    public const string RequestParameterEnumValueRemoved = "Request parameter enum value removed";
    public const string RequestBodyBecameRequired = "Request body became required";
    public const string ResponsePropertyBecameOptional = "Response property became optional";
    public const string ResponsePropertyBecameNullable = "Response property became nullable";
    public const string ResponseSuccessStatusRemoved = "Response success status removed";
    public const string ResponseMediaTypeRemoved = "Response media type removed";
    public const string RequiredResponseHeaderRemoved = "Required response header removed";
    public const string EndpointAdded = "Endpoint added";
    public const string EndpointRemoved = "Endpoint removed";
    public const string SchemaAdded = "Schema added";
    public const string SchemaRemoved = "Schema removed";
    public const string SchemaRenamed = "Schema renamed";
    public const string DescriptionChanged = "Description changed";
}
