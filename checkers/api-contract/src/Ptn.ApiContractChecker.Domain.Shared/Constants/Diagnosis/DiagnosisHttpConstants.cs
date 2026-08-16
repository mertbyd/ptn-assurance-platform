namespace Ptn.ApiContractChecker.Constants.Diagnosis;

// islevi: Teshiste okunan standart HTTP header, auth hata ve safe method kodlarini sahiplenir.
// sistemdeki gorevi: Extractor ve adapter siniflarinda kararli protokol literallerinin dagilmasini engeller.
public static class DiagnosisHttpConstants
{
    public const string WwwAuthenticate = "WWW-Authenticate";
    public const string Allow = "Allow";
    public const string RetryAfter = "Retry-After";
    public const string ETag = "ETag";
    public const string Location = "Location";
    public const string Authorization = "Authorization";
    public const string Bearer = "Bearer";
    public const string InvalidToken = "invalid_token";
    public const string InsufficientScope = "insufficient_scope";
    public const string RequiredErrorCode = "required";
    public const string ValueErrorToken = "value";
    public const string ErrorParameter = "error";
    public const string ScopeParameter = "scope";
    public const string Get = "GET";
    public const string Head = "HEAD";
    public const string Options = "OPTIONS";
}
