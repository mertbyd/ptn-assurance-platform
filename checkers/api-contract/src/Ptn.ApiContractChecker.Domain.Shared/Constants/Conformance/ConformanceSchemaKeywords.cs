namespace Ptn.ApiContractChecker.Constants.Conformance;

// islevi: Validator tanilarinin dis kontrata cikan kararli JSON Schema anahtar sozcuklerini tanimlar.
// sistemdeki gorevi: Kutuphane enumlarini RFC 6901 ihlal sonucundan ayirir ve inline schema tokenlarini engeller.
public static class ConformanceSchemaKeywords
{
    public const string Unknown = "unknown";
    public const string Type = "type";
    public const string Required = "required";
    public const string AdditionalProperties = "additionalProperties";
    public const string Enum = "enum";
    public const string Pattern = "pattern";
    public const string MinLength = "minLength";
    public const string MaxLength = "maxLength";
    public const string Minimum = "minimum";
    public const string Maximum = "maximum";
    public const string MinItems = "minItems";
    public const string MaxItems = "maxItems";
    public const string UniqueItems = "uniqueItems";
    public const string AnyOf = "anyOf";
    public const string AllOf = "allOf";
    public const string OneOf = "oneOf";
    public const string Not = "not";
    public const string StatusCode = "statusCode";
    public const string ContentType = "contentType";
    public const string Headers = "headers";
    public const string Security = "security";
}
