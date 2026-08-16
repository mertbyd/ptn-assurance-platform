using System.Globalization;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Models.Comparison.Findings;
using Volo.Abp.DependencyInjection;

namespace Ptn.DatabaseChecker.Managers.Comparison;

// islevi: Siddetlendirilmemis veritabani bulgusunu tur, yon, rol ve guven bilgisine gore siniflandirir.
// sistemdeki gorevi: Breaking/NonBreaking/Warning/DocsOnly kararlarini diff ureticilerinden ayiran tek saf karar noktadir.
/// <summary>
/// Veritabani bulgularini taraf rolu ve degisim kanitina gore siniflandirir.
/// </summary>
public class DifferenceSeverityClassifier : ITransientDependency
{
    /// <summary>
    /// Yapisal bulguyu kaynak taraf rolu ve degisim kanitina gore siniflandirir.
    /// </summary>
    public string Classify(SchemaDifferenceModel difference, string sourceRoleCode)
    {
        if (HasLowConfidence(difference))
        {
            return DifferenceSeverityCodes.Warning;
        }

        var severity = difference.KindCode switch
        {
            DifferenceKindCodes.OnlyInSource => ClassifyPresence(difference, sourceRoleCode, true),
            DifferenceKindCodes.OnlyInTarget => ClassifyPresence(difference, sourceRoleCode, false),
            DifferenceKindCodes.Modified => ClassifyModification(difference, sourceRoleCode),
            _ => DifferenceSeverityCodes.Warning
        };
        return severity;
    }

    /// <summary>
    /// Veri bulgusunu eksik taraf rolune gore siniflandirir.
    /// </summary>
    public string Classify(DataDifferenceModel difference, string sourceRoleCode)
        => IsAuditedSideMissing(difference.KindCode, sourceRoleCode)
            ? DifferenceSeverityCodes.Breaking
            : DifferenceSeverityCodes.Warning;

    /// <summary>
    /// Migration bulgusunu eksik taraf rolune gore siniflandirir.
    /// </summary>
    public string Classify(MigrationDifferenceModel difference, string sourceRoleCode)
        => IsAuditedSideMissing(difference.KindCode, sourceRoleCode)
            ? DifferenceSeverityCodes.Breaking
            : DifferenceSeverityCodes.Warning;

    // islevi: Tek tarafli nesne farkini eksik taraf ve nesne ailesine gore siniflandirir.
    private static string ClassifyPresence(
        SchemaDifferenceModel difference,
        string sourceRoleCode,
        bool presentInSource)
    {
        var presentRole = presentInSource
            ? sourceRoleCode
            : ComparisonSideRoleCodes.Opposite(sourceRoleCode);
        if (presentRole == ComparisonSideRoleCodes.Reference)
        {
            return DifferenceSeverityCodes.Breaking;
        }

        return ClassifyAuditedAddition(difference, presentInSource);
    }

    // islevi: Denetlenen taraftaki yeni nesnenin ekleme uyumlulugunu siniflandirir.
    private static string ClassifyAuditedAddition(SchemaDifferenceModel difference, bool presentInSource)
    {
        if (difference.ObjectTypeCode == SchemaObjectTypeCodes.Column)
        {
            var definition = presentInSource ? difference.SourceDefinition : difference.TargetDefinition;
            return ClassifyAuditedColumnAddition(definition);
        }

        return DifferenceSeverityCodes.NonBreaking;
    }

    // islevi: Yeni kolonun nullable veya default'lu olmasini geriye uyumlu, default'suz NOT NULL olmasini breaking sayar.
    private static string ClassifyAuditedColumnAddition(string? definition)
    {
        var isNullable = ReadBoolean(definition, SchemaComparisonTextConstants.DefinitionFields.Nullable);
        var defaultValue = ReadField(definition, SchemaComparisonTextConstants.DefinitionFields.Default);
        return isNullable == false && string.IsNullOrWhiteSpace(defaultValue)
            ? DifferenceSeverityCodes.Breaking
            : DifferenceSeverityCodes.NonBreaking;
    }

    // islevi: Iki tarafta bulunan nesnenin alan degisimlerini aileye ozel kurala yonlendirir.
    private static string ClassifyModification(SchemaDifferenceModel difference, string sourceRoleCode)
    {
        if (IsDocumentationOnly(difference.ChangeSummary))
        {
            return DifferenceSeverityCodes.DocsOnly;
        }

        return difference.ObjectTypeCode == SchemaObjectTypeCodes.Column
            ? ClassifyColumnModification(difference, sourceRoleCode)
            : DifferenceSeverityCodes.Warning;
    }

    // islevi: Kolon degisimini reference ve audited tanimlarini ayirarak daralma/genisleme kurallarina uygular.
    private static string ClassifyColumnModification(
        SchemaDifferenceModel difference,
        string sourceRoleCode)
    {
        var reference = sourceRoleCode == ComparisonSideRoleCodes.Reference
            ? difference.SourceDefinition
            : difference.TargetDefinition;
        var audited = sourceRoleCode == ComparisonSideRoleCodes.Reference
            ? difference.TargetDefinition
            : difference.SourceDefinition;
        if (BecameRequired(reference, audited) || IsNarrower(reference, audited))
        {
            return DifferenceSeverityCodes.Breaking;
        }

        return ClassifyTypeChange(reference, audited);
    }

    // islevi: Kanonik tip degisimini bilinen genisleme/daralma ailelerine gore siniflandirir.
    private static string ClassifyTypeChange(string? reference, string? audited)
    {
        var referenceType = ReadField(reference, SchemaComparisonTextConstants.DefinitionFields.CanonicalType);
        var auditedType = ReadField(audited, SchemaComparisonTextConstants.DefinitionFields.CanonicalType);
        if (string.IsNullOrWhiteSpace(referenceType) || referenceType == auditedType)
        {
            return DifferenceSeverityCodes.NonBreaking;
        }

        return ClassifyKnownTypeChange(referenceType, auditedType);
    }

    // islevi: Integral, floating-point ve metin ailelerindeki yonlu kapasite degisimini cozer.
    private static string ClassifyKnownTypeChange(string referenceType, string? auditedType)
    {
        var widening = IsKnownWidening(referenceType, auditedType);
        return widening switch
        {
            true => DifferenceSeverityCodes.NonBreaking,
            false => DifferenceSeverityCodes.Breaking,
            null => DifferenceSeverityCodes.Warning
        };
    }

    // islevi: Bilinen ayni-aile tip degisiminin genisleme mi daralma mi oldugunu aile resolver'larina yonlendirir.
    private static bool? IsKnownWidening(string referenceType, string? auditedType)
    {
        var integralChange = ClassifyIntegralTypeChange(referenceType, auditedType);
        if (integralChange.HasValue)
        {
            return integralChange;
        }

        var floatingPointChange = ClassifyFloatingPointTypeChange(referenceType, auditedType);
        return floatingPointChange ?? ClassifyTextTypeChange(referenceType, auditedType);
    }

    // islevi: Yalniz integral aile icindeki kapasite yonunu rank degeriyle cozer.
    private static bool? ClassifyIntegralTypeChange(string referenceType, string? auditedType)
        => ClassifyRankedFamily(GetIntegralRank(referenceType), GetIntegralRank(auditedType));

    // islevi: Yalniz floating-point aile icindeki kapasite yonunu rank degeriyle cozer.
    private static bool? ClassifyFloatingPointTypeChange(string referenceType, string? auditedType)
        => ClassifyRankedFamily(GetFloatingPointRank(referenceType), GetFloatingPointRank(auditedType));

    // islevi: Yalniz metin aile icindeki kapasite yonunu rank degeriyle cozer.
    private static bool? ClassifyTextTypeChange(string referenceType, string? auditedType)
        => ClassifyRankedFamily(GetTextRank(referenceType), GetTextRank(auditedType));

    // islevi: Ayni bilinen ailede hedef rank'in kaynak rank'ten buyuk olup olmadigini bildirir.
    private static bool? ClassifyRankedFamily(int referenceRank, int auditedRank)
        => referenceRank == 0 || auditedRank == 0 ? null : auditedRank > referenceRank;

    // islevi: Integral tipleri kucukten buyuge kararli kapasite sirasina cevirir.
    private static int GetIntegralRank(string? typeCode)
        => typeCode switch
        {
            CanonicalDataTypeCodes.SmallInteger => 1,
            CanonicalDataTypeCodes.Integer => 2,
            CanonicalDataTypeCodes.BigInteger => 3,
            _ => 0
        };

    // islevi: Floating-point tipleri kucukten buyuge kararli kapasite sirasina cevirir.
    private static int GetFloatingPointRank(string? typeCode)
        => typeCode switch
        {
            CanonicalDataTypeCodes.Float => 1,
            CanonicalDataTypeCodes.Double => 2,
            _ => 0
        };

    // islevi: Metin tiplerini kucukten buyuge kararli kapasite sirasina cevirir.
    private static int GetTextRank(string? typeCode)
        => typeCode switch
        {
            CanonicalDataTypeCodes.String => 1,
            CanonicalDataTypeCodes.Text => 2,
            _ => 0
        };

    // islevi: Nullable reference kolonunun audited tarafta NOT NULL olmasini bulur.
    private static bool BecameRequired(string? reference, string? audited)
        => ReadBoolean(reference, SchemaComparisonTextConstants.DefinitionFields.Nullable) == true &&
           ReadBoolean(audited, SchemaComparisonTextConstants.DefinitionFields.Nullable) == false;

    // islevi: Uzunluk veya sayisal precision azalmasini tip daralmasi olarak belirler.
    private static bool IsNarrower(string? reference, string? audited)
    {
        var referenceMax = ReadInteger(reference, SchemaComparisonTextConstants.DefinitionFields.MaxLength);
        var auditedMax = ReadInteger(audited, SchemaComparisonTextConstants.DefinitionFields.MaxLength);
        var referencePrecision = ReadInteger(reference, SchemaComparisonTextConstants.DefinitionFields.NumericPrecision);
        var auditedPrecision = ReadInteger(audited, SchemaComparisonTextConstants.DefinitionFields.NumericPrecision);
        return IsReduced(referenceMax, auditedMax) || IsReduced(referencePrecision, auditedPrecision);
    }

    // islevi: Dolu reference kapasitesinin audited tarafta daha kucuk olup olmadigini bildirir.
    private static bool IsReduced(int? reference, int? audited)
        => reference.HasValue && audited.HasValue && audited.Value < reference.Value;

    // islevi: Yalniz comment etiketinden olusan degisimi dokumantasyon farki olarak tanir.
    private static bool IsDocumentationOnly(string? changeSummary)
        => string.Equals(
            changeSummary?.Trim(),
            SchemaComparisonTextConstants.ChangeLabels.Comment,
            StringComparison.Ordinal);

    // islevi: Approximate veya Incomparable guvenli tum yapisal farklari insan incelemesine yonlendirir.
    private static bool HasLowConfidence(SchemaDifferenceModel difference)
        => difference.ConfidenceCode is ComparisonConfidenceCodes.Approximate or
            ComparisonConfidenceCodes.Incomparable;

    // islevi: OnlyIn yon kodundan audited tarafin eksik olup olmadigini cozer.
    private static bool IsAuditedSideMissing(string kindCode, string sourceRoleCode)
        => kindCode == DifferenceKindCodes.OnlyInSource
            ? ComparisonSideRoleCodes.Opposite(sourceRoleCode) == ComparisonSideRoleCodes.Audited
            : kindCode == DifferenceKindCodes.OnlyInTarget &&
              sourceRoleCode == ComparisonSideRoleCodes.Audited;

    // islevi: Kanonik definition icindeki boolean alani okur.
    private static bool? ReadBoolean(string? definition, string fieldName)
        => bool.TryParse(ReadField(definition, fieldName), out var value) ? value : null;

    // islevi: Kanonik definition icindeki integer alani invariant olarak okur.
    private static int? ReadInteger(string? definition, string fieldName)
        => int.TryParse(ReadField(definition, fieldName), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;

    // islevi: Noktali-virgullu kanonik definition metninden adlandirilmis alan degerini ayiklar.
    private static string? ReadField(string? definition, string fieldName)
        => definition?
            .Split(ComparisonCanonicalTextConstants.DefinitionFieldSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Select(field => field.Split(ComparisonCanonicalTextConstants.DefinitionKeyValueSeparator, 2))
            .FirstOrDefault(parts => parts.Length == 2 && parts[0] == fieldName)?[1];
}
