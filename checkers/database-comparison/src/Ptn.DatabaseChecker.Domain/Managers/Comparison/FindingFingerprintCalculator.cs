using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Models.Comparison.Findings;
using Volo.Abp.DependencyInjection;

namespace Ptn.DatabaseChecker.Managers.Comparison;

// islevi: Bulgu adresi ve normalize edilmis delta bilgisinden kararli SHA-256 parmak izi uretir.
// sistemdeki gorevi: Run kimligi ve bulgu listesindeki siradan bagimsiz tekrar bulgu kimligini tek saf hesaplayicida tutar.
/// <summary>
/// Bulgu adresi ve deltasindan kararli SHA-256 parmak izi hesaplar.
/// </summary>
public class FindingFingerprintCalculator : ITransientDependency
{
    /// <summary>
    /// Yapisal bulgunun motor cifti, adresi, turu ve delta kanitindan parmak izi hesaplar.
    /// </summary>
    public string Calculate(
        string sourceEngineCode,
        string targetEngineCode,
        SchemaDifferenceModel difference)
        => Compute(
            sourceEngineCode,
            targetEngineCode,
            difference.SchemaName,
            difference.ObjectTypeCode,
            difference.ObjectName,
            difference.ChildName,
            difference.KindCode,
            NormalizeDelta(difference.SourceDefinition, difference.TargetDefinition, difference.ChangeSummary));

    /// <summary>
    /// Veri bulgusunun motor cifti, tablo adresi, yonu ve sayim/hash deltasindan parmak izi hesaplar.
    /// </summary>
    public string Calculate(
        string sourceEngineCode,
        string targetEngineCode,
        DataDifferenceModel difference)
        => Compute(
            sourceEngineCode,
            targetEngineCode,
            difference.SchemaName,
            SchemaObjectTypeCodes.Table,
            difference.TableName,
            null,
            difference.KindCode,
            BuildDataDelta(difference));

    /// <summary>
    /// Migration bulgusunun motor cifti, defter adresi, yonu ve surum deltasindan parmak izi hesaplar.
    /// </summary>
    public string Calculate(
        string sourceEngineCode,
        string targetEngineCode,
        MigrationDifferenceModel difference)
        => Compute(
            sourceEngineCode,
            targetEngineCode,
            difference.SourceSchemaName ?? difference.TargetSchemaName,
            SchemaObjectTypeCodes.Migration,
            difference.MigrationId,
            null,
            difference.KindCode,
            NormalizeDelta(difference.SourceProductVersion, difference.TargetProductVersion));

    // islevi: Parmak izi protokolunun tum alanlarini uzunluk-etiketli tek kanonik metinde birlestirir.
    private static string Compute(
        string sourceEngineCode,
        string targetEngineCode,
        string? schemaName,
        string objectTypeCode,
        string objectName,
        string? childName,
        string kindCode,
        string delta)
    {
        var canonical = string.Concat(
            FindingAddressGrammar.BuildFingerprintAddress(
                sourceEngineCode,
                targetEngineCode,
                schemaName,
                objectTypeCode,
                objectName,
                childName),
            FindingAddressGrammar.EncodeComponent(kindCode),
            FindingAddressGrammar.EncodeComponent(delta));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }

    // islevi: Veri farkinin sayim, hash ve sirali satir/hucre kanitini kararli delta metnine cevirir.
    private static string BuildDataDelta(DataDifferenceModel difference)
        => NormalizeDelta(
            FormatNumber(difference.SourceRowCount),
            FormatNumber(difference.TargetRowCount),
            FormatNumber(difference.RowCountDifference),
            difference.SourceHash,
            difference.TargetHash,
            BuildRowDelta(difference));

    // islevi: Satir ve hucre farklarini girdi sirasindan bagimsiz kanonik metne cevirir.
    private static string BuildRowDelta(DataDifferenceModel difference)
        => string.Join(
            ComparisonCanonicalTextConstants.RowSeparator,
            difference.RowDifferences
                .OrderBy(row => row.PrimaryKeyValue, StringComparer.Ordinal)
                .ThenBy(row => row.KindCode, StringComparer.Ordinal)
                .Select(row => NormalizeDelta(
                    row.PrimaryKeyValue,
                    row.KindCode,
                    string.Join(
                        ComparisonCanonicalTextConstants.FieldSeparator,
                        row.ValueDifferences
                            .OrderBy(value => value.ColumnName, StringComparer.Ordinal)
                            .Select(value => NormalizeDelta(
                                value.ColumnName,
                                value.SourceValue,
                                value.TargetValue))))));

    // islevi: Nullable sayiyi kulturden bagimsiz delta parcasina cevirir.
    private static string? FormatNumber(long? value)
        => value?.ToString(CultureInfo.InvariantCulture);

    // islevi: Delta parcalarinda satir sonu ve gereksiz bosluk gurultusunu normalize eder.
    private static string NormalizeDelta(params string?[] values)
        => string.Concat(values.Select(value => FindingAddressGrammar.EncodeComponent(NormalizeText(value))));

    // islevi: Metni trimleyip tum whitespace kosularini tek bosluga indirger.
    private static string? NormalizeText(string? value)
        => value is null
            ? null
            : string.Join(' ', value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

}
