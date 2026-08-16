using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Constants.Comparison.Assertions;
using Ptn.DatabaseChecker.Models.Comparison;
using Volo.Abp.DependencyInjection;

namespace Ptn.DatabaseChecker.Managers.Comparison;

// islevi: Iki kolonun motor, kanonik tip ve fidelity bilgisinden fark guven kodunu saf olarak cozer.
// sistemdeki gorevi: Kolon-bazli Exact/Canonical/Approximate/Incomparable kararinin comparer cagri noktalarinda tekrar edilmesini engeller.
public class ColumnTypeConfidenceResolver : ITransientDependency
{
    // islevi: Assertion matcher'inin kanonik kolon ailesinde guvenle degerlendirilebilir olup olmadigini bildirir.
    public bool IsMatcherCompatible(string matcherCode, string canonicalTypeCode)
        => matcherCode switch
        {
            MatcherKindCodes.IsNull or MatcherKindCodes.IsNotNull => true,
            MatcherKindCodes.Equals or MatcherKindCodes.NotEquals or MatcherKindCodes.OneOf
                => canonicalTypeCode != CanonicalDataTypeCodes.Unknown,
            MatcherKindCodes.MatchesRegex => IsTextual(canonicalTypeCode),
            MatcherKindCodes.WithinTolerance => IsNumeric(canonicalTypeCode),
            MatcherKindCodes.GreaterThan or MatcherKindCodes.GreaterThanOrEqual
                or MatcherKindCodes.LessThan or MatcherKindCodes.LessThanOrEqual
                => IsNumeric(canonicalTypeCode) || IsTemporal(canonicalTypeCode) || IsTextual(canonicalTypeCode),
            _ => false
        };

    // islevi: Ayni motorda ham tipe, capraz motorda tasinan kanonik esleme sonucuna gore guven kodu uretir.
    public string Resolve(
        string sourceEngineCode,
        string targetEngineCode,
        SchemaColumnModel? sourceColumn,
        SchemaColumnModel? targetColumn)
    {
        if (string.Equals(sourceEngineCode, targetEngineCode, StringComparison.Ordinal))
        {
            return ComparisonConfidenceCodes.Exact;
        }

        if (!HasUsableMapping(sourceColumn) || !HasUsableMapping(targetColumn))
        {
            return ComparisonConfidenceCodes.Incomparable;
        }

        return IsApproximate(sourceColumn!) || IsApproximate(targetColumn!)
            ? ComparisonConfidenceCodes.Approximate
            : ComparisonConfidenceCodes.Canonical;
    }

    // islevi: Kolonda hem kanonik aile hem de tanimli fidelity kodu tasindigini dogrular.
    private static bool HasUsableMapping(SchemaColumnModel? column)
        => column is not null &&
           !string.IsNullOrWhiteSpace(column.CanonicalDataType) &&
           (string.Equals(
                column.TypeMappingFidelityCode,
                TypeMappingFidelityCodes.Exact,
                StringComparison.Ordinal) ||
            string.Equals(
                column.TypeMappingFidelityCode,
                TypeMappingFidelityCodes.Approximate,
                StringComparison.Ordinal));

    // islevi: Tek tarafin kanonik tipe kayipli donusturuldugunu bildirir.
    private static bool IsApproximate(SchemaColumnModel column)
        => string.Equals(
            column.TypeMappingFidelityCode,
            TypeMappingFidelityCodes.Approximate,
            StringComparison.Ordinal);

    // islevi: Kanonik tip kodunun sayisal assertion ailesinde olup olmadigini bildirir.
    private static bool IsNumeric(string code)
        => code is CanonicalDataTypeCodes.Integer or CanonicalDataTypeCodes.SmallInteger
            or CanonicalDataTypeCodes.BigInteger or CanonicalDataTypeCodes.Decimal
            or CanonicalDataTypeCodes.Float or CanonicalDataTypeCodes.Double or CanonicalDataTypeCodes.Money;

    // islevi: Kanonik tip kodunun zaman assertion ailesinde olup olmadigini bildirir.
    private static bool IsTemporal(string code)
        => code is CanonicalDataTypeCodes.Date or CanonicalDataTypeCodes.Time
            or CanonicalDataTypeCodes.Timestamp or CanonicalDataTypeCodes.TimestampWithTimeZone;

    // islevi: Kanonik tip kodunun ordinal veya regex metin ailesinde olup olmadigini bildirir.
    private static bool IsTextual(string code)
        => code is CanonicalDataTypeCodes.String or CanonicalDataTypeCodes.Text or CanonicalDataTypeCodes.Enum;
}
