using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Managers.Comparison;
using Ptn.DatabaseChecker.Models.Comparison;
using Volo.Abp.DependencyInjection;

namespace Ptn.DatabaseChecker.Managers.SchemaDiscovery;

// islevi: Sema fotografini kolon -> tablo -> sema -> snapshot sirasiyla kararli SHA-256 Merkle muhrune indirger.
// sistemdeki gorevi: Saf hesaplayicidir; I/O yapmaz, saat okumaz ve fotografi saklamaz. Zaman, istatistik ve okuma sirasi muhre girmedigi icin ayni yapi her koc'ta bit duzeyinde ayni muhru uretir.
/// <summary>
/// Sema fotografindan kanonik SHA-256 Merkle parmak izi hesaplar.
/// </summary>
public class SchemaFingerprintCalculator : ITransientDependency
{
    // Tanim/ifade/identifier gurultusunu eleyen ev normalizasyon sahibi; kiyas motoruyla ayni kurallari paylasir.
    private readonly SchemaDefinitionNormalizer _normalizer;

    // islevi: Hesaplayiciyi mevcut sema normalizasyon sahibiyle kurar.
    public SchemaFingerprintCalculator(SchemaDefinitionNormalizer normalizer)
    {
        _normalizer = normalizer;
    }

    /// <summary>
    /// Fotografin yapisindan snapshot muhrunu ve kayan dali gosteren sema/tablo dal muhurlerini uretir.
    /// </summary>
    public SchemaFingerprintModel Calculate(SchemaSnapshotModel snapshot)
    {
        var tables = ComputeTableBranches(snapshot);
        var schemas = BuildSchemaEntries(snapshot, tables);
        return new SchemaFingerprintModel
        {
            AlgorithmCode = SchemaFingerprintConsts.AlgorithmCode,
            AlgorithmVersion = SchemaFingerprintConsts.AlgorithmVersion,
            SnapshotFingerprint = ComputeSnapshotFingerprint(snapshot, schemas),
            Schemas = schemas,
            Tables = tables
                .Select(branch => new SchemaFingerprintEntryModel
                {
                    Name = branch.Address,
                    Fingerprint = branch.Fingerprint
                })
                .ToList()
        };
    }

    // islevi: Her tabloyu semasi, kararli adresi ve muhruyle adres sirasina gore tek gecise indirger.
    private List<(string Schema, string Address, string Fingerprint)> ComputeTableBranches(SchemaSnapshotModel snapshot)
        => snapshot.Tables
            .Select(table => (
                table.Schema,
                Address: FindingAddressGrammar.FormatTargetAddress(table.Schema, table.Name, null),
                Fingerprint: ComputeTableFingerprint(table, snapshot.DatabaseCollationName)))
            .OrderBy(branch => branch.Address, StringComparer.Ordinal)
            .ToList();

    // islevi: Tablo ve tablo-disi nesne dallarini semalarina gore toplayip her sema icin dal muhru uretir.
    private List<SchemaFingerprintEntryModel> BuildSchemaEntries(
        SchemaSnapshotModel snapshot,
        List<(string Schema, string Address, string Fingerprint)> tables)
    {
        var tablesBySchema = GroupBySchema(tables, branch => branch.Schema, branch => branch.Fingerprint);
        var objectsBySchema = GroupBySchema(snapshot.Objects, definition => definition.Schema, ComputeObjectText);
        return tablesBySchema.Keys
            .Concat(objectsBySchema.Keys)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(schemaName => schemaName, StringComparer.Ordinal)
            .Select(schemaName => new SchemaFingerprintEntryModel
            {
                Name = schemaName,
                Fingerprint = ComputeSchemaFingerprint(
                    schemaName,
                    ReadBranch(tablesBySchema, schemaName),
                    ReadBranch(objectsBySchema, schemaName))
            })
            .ToList();
    }

    // islevi: Kaynak dallari sema adina gore ordinal sozlukte toplar.
    private static Dictionary<string, List<string>> GroupBySchema<TSource>(
        IEnumerable<TSource> source,
        Func<TSource, string> schemaSelector,
        Func<TSource, string> partSelector)
        => source
            .GroupBy(schemaSelector, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Select(partSelector).ToList(),
                StringComparer.Ordinal);

    // islevi: Sema karsiligi olmayan dal kumesini bos listeye indirger.
    private static List<string> ReadBranch(Dictionary<string, List<string>> branches, string schemaName)
        => branches.TryGetValue(schemaName, out var parts) ? parts : new List<string>();

    // islevi: Motor kodunu, veritabani collation kimligini ve sirali sema muhurlerini tek snapshot muhrune indirger.
    private string ComputeSnapshotFingerprint(
        SchemaSnapshotModel snapshot,
        List<SchemaFingerprintEntryModel> schemas)
        => Hash(
            SchemaFingerprintConsts.Levels.Snapshot,
            SchemaFingerprintConsts.AlgorithmCode,
            SchemaFingerprintConsts.AlgorithmVersion.ToString(CultureInfo.InvariantCulture),
            snapshot.EngineCode,
            _normalizer.NormalizeIdentifier(snapshot.DatabaseCollationName),
            snapshot.CollationProviderCode,
            JoinSorted(schemas.Select(entry => entry.Fingerprint)));

    // islevi: Sema adini, sirali tablo muhurlerini ve tablo-disi nesne tanimlarini tek sema muhrune indirger.
    private string ComputeSchemaFingerprint(
        string schemaName,
        List<string> tableFingerprints,
        List<string> objectTexts)
        => Hash(
            SchemaFingerprintConsts.Levels.Schema,
            _normalizer.NormalizeIdentifier(schemaName),
            JoinSorted(tableFingerprints),
            JoinSorted(objectTexts));

    // islevi: Tablo adresini, sirali kolon muhurlerini ve kisit/index/trigger tanimlarini tek tablo muhrune indirger.
    private string ComputeTableFingerprint(SchemaTableModel table, string? databaseCollation)
        => Hash(
            SchemaFingerprintConsts.Levels.Table,
            _normalizer.NormalizeIdentifier(table.Schema),
            _normalizer.NormalizeIdentifier(table.Name),
            JoinSorted(table.Columns.Select(column => ComputeColumnFingerprint(column, databaseCollation))),
            JoinSorted(table.Constraints.Select(ComputeConstraintText)),
            JoinSorted(table.Indexes.Select(ComputeIndexText)),
            JoinSorted(table.Triggers.Select(ComputeTriggerText)));

    // islevi: Kolonun adini, tip seklini, nullability'sini, default/generated ifadesini, collation'ini ve identity bilgisini tek kolon muhrune indirger.
    private string ComputeColumnFingerprint(SchemaColumnModel column, string? databaseCollation)
        => Hash(
            SchemaFingerprintConsts.Levels.Column,
            _normalizer.NormalizeIdentifier(column.Name),
            column.RawDataType,
            column.CanonicalDataType,
            FormatNumber(column.MaxLength),
            FormatNumber(column.NumericPrecision),
            FormatNumber(column.NumericScale),
            FormatFlag(column.IsNullable),
            _normalizer.NormalizeExpression(column.DefaultValueSql),
            FormatFlag(column.IsGenerated),
            _normalizer.NormalizeExpression(column.GenerationExpression),
            FormatFlag(column.IsPersisted),
            _normalizer.NormalizeColumnCollation(column.CollationName, databaseCollation),
            FormatFlag(column.IsIdentity),
            column.IdentitySeed,
            column.IdentityIncrement);

    // islevi: Kisidin adini, turunu, kolon/hedef adresini, referans aksiyonlarini ve guven durumunu kanonik metne cevirir.
    private string ComputeConstraintText(SchemaConstraintModel constraint)
        => Encode(
            _normalizer.NormalizeIdentifier(constraint.Name),
            constraint.TypeCode,
            _normalizer.NormalizeNameList(constraint.Columns, sort: false),
            _normalizer.NormalizeIdentifier(constraint.ReferencedTable),
            _normalizer.NormalizeNameList(constraint.ReferencedColumns, sort: false),
            constraint.DeleteActionCode,
            constraint.UpdateActionCode,
            _normalizer.NormalizeExpression(constraint.Definition),
            FormatFlag(constraint.IsValidated),
            FormatFlag(constraint.IsEnabled),
            FormatFlag(constraint.IsDeferrable),
            FormatFlag(constraint.IsInitiallyDeferred));

    // islevi: Index'in adini, tekillik/PK bayraklarini, kolon listelerini ve filtre/tanim metnini kanonik metne cevirir.
    private string ComputeIndexText(SchemaIndexModel index)
        => Encode(
            _normalizer.NormalizeIdentifier(index.Name),
            FormatFlag(index.IsUnique),
            FormatFlag(index.IsPrimaryKey),
            _normalizer.NormalizeNameList(index.Columns, sort: false),
            _normalizer.NormalizeNameList(index.IncludedColumns, sort: true),
            _normalizer.NormalizeExpression(index.FilterDefinition),
            _normalizer.NormalizeDefinition(index.Definition));

    // islevi: Trigger'in adini, tanimini ve etkinlik durumunu kanonik metne cevirir.
    private string ComputeTriggerText(SchemaTriggerModel trigger)
        => Encode(
            _normalizer.NormalizeIdentifier(trigger.Name),
            _normalizer.NormalizeDefinition(trigger.Definition),
            FormatFlag(trigger.IsEnabled));

    // islevi: Tablo disi sema nesnesinin adini, turunu ve tanimini kanonik metne cevirir.
    private string ComputeObjectText(SchemaObjectDefinitionModel definition)
        => Encode(
            _normalizer.NormalizeIdentifier(definition.Name),
            definition.ObjectTypeCode,
            _normalizer.NormalizeDefinition(definition.Definition));

    // islevi: Seviye etiketli kanonik metnin buyuk harfli onaltilik SHA-256 ozetini uretir.
    private static string Hash(string levelCode, params string?[] components)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
            FindingAddressGrammar.EncodeComponent(levelCode) + Encode(components))));

    // islevi: Bilesenleri mevcut uzunluk-etiketli, null-guvenli protokolle tek kanonik metinde birlestirir.
    private static string Encode(params string?[] components)
        => string.Concat(components.Select(FindingAddressGrammar.EncodeComponent));

    // islevi: Alt nesne parcalarini okuma sirasindan bagimsiz kararli tek metne indirger.
    private static string JoinSorted(IEnumerable<string> parts)
        => string.Join(
            ComparisonCanonicalTextConstants.FieldSeparator,
            parts.OrderBy(part => part, StringComparer.Ordinal));

    // islevi: Bayrak degerini kulturden bagimsiz kararli metne cevirir.
    private static string FormatFlag(bool value)
        => value.ToString();

    // islevi: Nullable sayiyi kulturden bagimsiz metne cevirir; null bilinmeyen olarak sifirdan ayri kodlanir.
    private static string? FormatNumber(int? value)
        => value?.ToString(CultureInfo.InvariantCulture);
}
