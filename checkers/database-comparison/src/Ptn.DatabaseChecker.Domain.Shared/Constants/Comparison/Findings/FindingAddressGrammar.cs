namespace Ptn.DatabaseChecker.Constants.Comparison;

// islevi: Database bulgu adresini okunabilir yol ve fingerprint girdisi olarak tek kanonik gramerle uretir.
// sistemdeki gorevi: Domain hesaplayicisi ile public consumer'larin schema/object/child bilesenlerini ayni sirada kodlamasini saglar.
public static class FindingAddressGrammar
{
    /// <summary>Fingerprint adres bilesenlerinin degismez sirasi.</summary>
    public const string FingerprintComponentOrder =
        "SourceEngineCode,TargetEngineCode,SchemaName,ObjectTypeCode,ObjectName,ChildName";

    /// <summary>Okunabilir yolda eksik semayi acikca gosteren yer tutucu.</summary>
    public const string DefaultSchemaPlaceholder = "<default>";

    /// <summary>Okunabilir yolda bos bir bileseni acikca gosteren yer tutucu.</summary>
    public const string EmptyComponentPlaceholder = "<empty>";

    /// <summary>
    /// Fingerprint adresini mevcut uzunluk-etiketli, null-guvenli protokolle kurar.
    /// </summary>
    public static string BuildFingerprintAddress(
        string sourceEngineCode,
        string targetEngineCode,
        string? schemaName,
        string objectTypeCode,
        string objectName,
        string? childName)
        => string.Concat(
            EncodeComponent(sourceEngineCode),
            EncodeComponent(targetEngineCode),
            EncodeComponent(schemaName),
            EncodeComponent(objectTypeCode),
            EncodeComponent(objectName),
            EncodeComponent(childName));

    /// <summary>
    /// Adresi schema.object veya schema.object.child biciminde, gerekirse SQL cift tirnaklariyla gosterir.
    /// </summary>
    public static string FormatTargetAddress(string? schemaName, string objectName, string? childName)
    {
        var schema = FormatIdentifier(schemaName, DefaultSchemaPlaceholder);
        var objectPart = FormatIdentifier(objectName, EmptyComponentPlaceholder);
        return childName is null
            ? $"{schema}.{objectPart}"
            : $"{schema}.{objectPart}.{FormatIdentifier(childName, EmptyComponentPlaceholder)}";
    }

    /// <summary>
    /// Null, bos ve dolu metni birbiriyle carpisamayan uzunluk-etiketli protokol parcasina cevirir.
    /// </summary>
    public static string EncodeComponent(string? value)
        => value is null
            ? ComparisonCanonicalTextConstants.NullValueMarker
            : $"{ComparisonCanonicalTextConstants.ValueMarker}{value.Length}" +
              $"{ComparisonCanonicalTextConstants.LengthSeparator}{value}";

    // islevi: Basit kucuk-harfli identifier'i yalniz birakir; digerlerini cift tirnakla geri donuslu gosterir.
    private static string FormatIdentifier(string? value, string missingPlaceholder)
    {
        if (value is null)
        {
            return missingPlaceholder;
        }

        if (value.Length == 0)
        {
            return EmptyComponentPlaceholder;
        }

        return IsPlainIdentifier(value)
            ? value
            : $"\"{value.Replace("\"", "\"\"")}\"";
    }

    // islevi: SQL'de tirnaksiz yazildiginda case kaybetmeyen portable identifier seklini tanir.
    private static bool IsPlainIdentifier(string value)
    {
        if (!(value[0] is >= 'a' and <= 'z' || value[0] == '_'))
        {
            return false;
        }

        return value.Skip(1).All(character =>
            character is >= 'a' and <= 'z' ||
            character is >= '0' and <= '9' ||
            character == '_');
    }
}
