namespace Ptn.ApiContractChecker.Constants.Runs;

// islevi: Contract bulgu adresinin fingerprint bilesen sirasi ve kanonik metin kurallarini yayinlar.
// sistemdeki gorevi: Domain hesaplayicisi ile paket tuketicisinin operation fingerprint girdisini ayni bicimde kurmasini saglar.
public static class FindingAddressGrammar
{
    /// <summary>Adres bilesenlerinin fingerprint icindeki degismez sirasi.</summary>
    public const string ComponentOrder =
        "OperationId,HttpMethod,Path,SchemaName,PropertyPath,ParameterName,ResponseStatus,MediaType";

    /// <summary>Tum fingerprint bilesenlerinin degismez sirasi.</summary>
    public const string FingerprintComponentOrder =
        "KindCode,DirectionCode,OperationId,HttpMethod,Path,SchemaName,PropertyPath,ParameterName,ResponseStatus,MediaType,OldDelta,NewDelta";

    /// <summary>Bos veya eksik adres bilesenini fingerprint'te temsil eder.</summary>
    public const string EmptyComponent = "<empty>";

    /// <summary>Adres bilesenini entity kurucusuyla ayni trim/bosluk semantigine indirger.</summary>
    public static string Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? EmptyComponent : value.Trim();

    /// <summary>Normalize edilmis bileseni ayrac carpismasini onleyen uzunluk on ekiyle cerceveler.</summary>
    public static string Frame(string value) => $"{value.Length}:{value}";

    /// <summary>Sekiz typed adres bilesenini sabit sirada normalize edip dondurur.</summary>
    public static IReadOnlyList<string> BuildComponents(
        string? operationId,
        string? httpMethod,
        string? path,
        string? schemaName,
        string? propertyPath,
        string? parameterName,
        string? responseStatus,
        string? mediaType)
        =>
        [
            Normalize(operationId),
            Normalize(httpMethod),
            Normalize(path),
            Normalize(schemaName),
            Normalize(propertyPath),
            Normalize(parameterName),
            Normalize(responseStatus),
            Normalize(mediaType)
        ];
}
