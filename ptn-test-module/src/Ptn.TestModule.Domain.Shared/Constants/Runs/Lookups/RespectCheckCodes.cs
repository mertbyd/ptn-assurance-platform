using System.Collections.Generic;

namespace Ptn.TestModule.Constants.Runs.Lookups;

// islevi: Redocly Respect'in adim basina uyguladigi dort kontrolu kapali kodlarla tanimlar.
// sistemdeki gorevi: Severity haritasinin her kosumda dordunu birden acikca set etmesini saglar (AUDIT-0002 BULGU-08).
/// <summary>
/// Dis runner'in kendi adim kontrollerinin kararli kodlarini tasir.
/// </summary>
public static class RespectCheckCodes
{
    /// <summary>Adim yanit durum kodunun beklenenle eslesmesini kontrol eder.</summary>
    public const string StatusCodeCheck = "STATUS_CODE_CHECK";

    /// <summary>Adimin successCriteria ifadelerini kontrol eder.</summary>
    public const string SuccessCriteriaCheck = "SUCCESS_CRITERIA_CHECK";

    /// <summary>Adim yanit govdesinin OpenAPI semasina uygunlugunu kontrol eder.</summary>
    public const string SchemaCheck = "SCHEMA_CHECK";

    /// <summary>Adim yanit content-type basligini kontrol eder.</summary>
    public const string ContentTypeCheck = "CONTENT_TYPE_CHECK";

    /// <summary>Her kosumda severity'si acikca set edilmesi gereken tum kontrollerdir.</summary>
    public static IReadOnlyCollection<string> All { get; } =
        [StatusCodeCheck, SuccessCriteriaCheck, SchemaCheck, ContentTypeCheck];
}
