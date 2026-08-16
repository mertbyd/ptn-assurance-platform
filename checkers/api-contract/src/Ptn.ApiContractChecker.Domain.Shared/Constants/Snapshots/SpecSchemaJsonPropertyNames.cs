namespace Ptn.ApiContractChecker.Constants.Snapshots;

// islevi: Validator sema dugumu uretilirken kullanilan kararli JSON Schema property adlarini tanimlar.
// sistemdeki gorevi: Dialect bilesenlerinin schema anahtarlarini inline string olarak dagitmasini engeller.
public static class SpecSchemaJsonPropertyNames
{
    public const string Schema = "$schema";
    public const string Type = "type";
    public const string Nullable = "nullable";
    public const string SwaggerNullable = "x-nullable";
    public const string Format = "format";
    public const string Properties = "properties";
    public const string Required = "required";
    public const string Enum = "enum";
    public const string Pattern = "pattern";
    public const string MinLength = "minLength";
    public const string MaxLength = "maxLength";
    public const string Minimum = "minimum";
    public const string Maximum = "maximum";
    public const string MinItems = "minItems";
    public const string MaxItems = "maxItems";
    public const string UniqueItems = "uniqueItems";
    public const string Items = "items";
    public const string AdditionalProperties = "additionalProperties";
    public const string AnyOf = "anyOf";
    public const string OneOf = "oneOf";
    public const string Not = "not";
}
