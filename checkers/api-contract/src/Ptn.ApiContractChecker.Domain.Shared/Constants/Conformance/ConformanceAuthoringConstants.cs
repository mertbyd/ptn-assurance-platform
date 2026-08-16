namespace Ptn.ApiContractChecker.Constants.Conformance;

// islevi: Request ornegi ve operasyon baglama onerilerinin butce ve placeholder sabitlerini tanimlar.
// sistemdeki gorevi: Yazarlik yardimcilarinin uydurma is degeri veya inline limit uretmesini engeller.
public static class ConformanceAuthoringConstants
{
    public const int MaxRequestExampleBytes = 2048;
    public const int MaxBindingSuggestionBytes = 2048;
    public const int MaxBindingSuggestions = 5;
    public const int MaxRequestExampleDepth = 3;
    public const int MaxAssertionResultBytes = 512;
    public const int MaxAssertionPaths = 16;
    public const int MaxAssertionPathLength = 512;
    public const string StringPlaceholder = "string";
    public const string BindingArrow = " -> ";
    public const string BindingMemberSeparator = ".";
    public const string OperationReferenceSeparator = " ";
    public const string UuidPlaceholder = "00000000-0000-0000-0000-000000000000";
    public const string DatePlaceholder = "1970-01-01";
    public const string DateTimePlaceholder = "1970-01-01T00:00:00Z";
    public const string UuidFormat = "uuid";
    public const string GuidFormat = "guid";
    public const string DateFormat = "date";
    public const string DateTimeFormat = "date-time";
    public const string StringType = "string";
    public const string IntegerType = "integer";
    public const string NumberType = "number";
    public const string BooleanType = "boolean";
    public const string ArrayType = "array";
    public const string ObjectType = "object";
    public const string BodyPointerSegment = "body";
    public const char StringPaddingCharacter = 'x';
}
