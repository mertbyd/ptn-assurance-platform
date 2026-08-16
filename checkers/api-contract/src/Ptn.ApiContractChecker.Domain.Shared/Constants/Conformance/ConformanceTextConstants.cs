namespace Ptn.ApiContractChecker.Constants.Conformance;

// islevi: Response oracle'inin HTTP, path ve JSON pointer metin tokenlarini tanimlar.
// sistemdeki gorevi: Operation ve ihlal adresleme kurallarinin kod icinde dagilmasini engeller.
public static class ConformanceTextConstants
{
    public const string DefaultResponse = "default";
    public const char ContentTypeParameterSeparator = ';';
    public const char QuerySeparator = '?';
    public const char PathSeparator = '/';
    public const char TemplateStart = '{';
    public const char TemplateEnd = '}';
    public const string JsonPointerRoot = "";
    public const string JsonPointerSeparator = "/";
    public const string JsonPointerFragmentPrefix = "#/";
    public const string JsonPathRoot = "$";
    public const string JsonPathPropertySeparator = ".";
    public const string JsonPointerTilde = "~";
    public const string JsonPointerEscapedTilde = "~0";
    public const string JsonPointerEscapedSlash = "~1";
    public const string HeadersPointerSegment = "headers";
}
