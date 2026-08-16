using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.ExceptionCodes;
using Ptn.DatabaseChecker.Interface.Capabilities;
using Ptn.DatabaseChecker.Interface.Comparison;
using Ptn.DatabaseChecker.Interface.Projections;
using Ptn.DatabaseChecker.Models.Comparison;
using Ptn.DatabaseChecker.Models.Projections;
using Volo.Abp;

namespace Ptn.DatabaseChecker.Repository.Comparison;

// islevi: Motor-ozel veri/migration okuyuculari icin ortak "katalogu dogrula -> limit icin say -> satirlari batch oku" akis altyapisini saglar.
// sistemdeki gorevi: Migration ve tablo yapisi katalog context uzerinden okunur. Calisma-anindaki tablolarin kesin sayimi ve JSON satirlari, katalogda dogrulanmis/quote edilmis identifier'larla EF SqlQueryRaw uzerinden batch UNION ALL sorgularina cevrilir; elle DbConnection/DbCommand/reader yoktur.
public abstract partial class DatabaseDataComparisonRepositoryBase
    : IDatabaseDataComparisonRepository, IProjectionRepository, IWriteSetRepository
{
    /// <summary>
    /// Bu okuyucunun destekledigi lookup engine kodu.
    /// </summary>
    public abstract string EngineCode { get; }

    // islevi: Hedef veritabanina baglanan, sistem katalogu + __EFMigrationsHistory mapping'li migration-disi katalog context'ini kurar.
    protected abstract DbContext CreateCatalogContext(DatabaseConnectionInfo info);

    // islevi: __EFMigrationsHistory tablosunun provider varsayilan semasinda var olup olmadigini, zaten mapli sistem katalog satirlari uzerinden LINQ ile kontrol eder.
    protected abstract Task<bool> MigrationHistoryExistsAsync(DbContext catalogContext, CancellationToken cancellationToken);

    // islevi: Provider'in mapli __EFMigrationsHistory tablosunun sema adini finding zincirine tasir.
    protected abstract string MigrationHistorySchemaName { get; }

    // islevi: Row-count UNION ALL sorgusunun FROM parcasi icin provider identifier'ini guvenli tirnakli forma cevirir; SQL enjeksiyon yuzeyini kapatir.
    protected abstract string QuoteIdentifier(string identifier);

    // islevi: Kesin satir sayimini ureten provider ifadesi (PostgreSQL count(*)::bigint / SQL Server count_big(*)).
    protected abstract string CountExpression { get; }

    // islevi: Provider satir alias'ini tum kolonlari ve null degerleri iceren tek JSON metnine cevirir.
    protected abstract string BuildRowJsonExpression(string rowAlias);

    // islevi: Yalniz katalogda dogrulanmis secili kolonlari provider JSON nesnesine cevirir.
    protected virtual string BuildProjectedRowJsonExpression(
        string rowAlias,
        IReadOnlyList<string> projectColumns,
        List<object> parameters)
        => throw new NotSupportedException();

    // islevi: Secili adreslerden gercekte mevcut tablolari, kolonlari ve PK sirasini provider kataloglarindan batch okur.
    protected abstract Task<List<TableDataStructureModel>> ReadTableStructuresCoreAsync(
        DbContext catalogContext,
        List<ComparisonTableIdentifierModel> tables,
        CancellationToken cancellationToken);

    // islevi: __EFMigrationsHistory defterini katalog context uzerinden LINQ ile okur; tablo yoksa bos liste dondurur.
    public async Task<List<MigrationHistoryEntryModel>> ReadMigrationHistoryAsync(
        DatabaseConnectionInfo info,
        CancellationToken cancellationToken = default)
    {
        await using var catalogContext = CreateCatalogContext(info);

        // Tablo yoksa LINQ sorgusu firlatir; house kurali try/catch yerine varligi once LINQ ile dogrular.
        if (!await MigrationHistoryExistsAsync(catalogContext, cancellationToken))
        {
            return new List<MigrationHistoryEntryModel>();
        }

        return await catalogContext.Set<EfMigrationsHistoryCatalogRow>()
            .OrderBy(entry => entry.MigrationId)
            .Select(entry => new MigrationHistoryEntryModel
            {
                SchemaName = MigrationHistorySchemaName,
                MigrationId = entry.MigrationId,
                ProductVersion = entry.ProductVersion
            })
            .ToListAsync(cancellationToken);
    }

    // islevi: Secili tablolarin kesin row-count degerlerini tek batch UNION ALL sorgusuyla katalog context uzerinden okur.
    public async Task<List<TableRowCountModel>> ReadRowCountsAsync(
        DatabaseConnectionInfo info,
        List<ComparisonTableIdentifierModel> tables,
        CancellationToken cancellationToken = default)
    {
        if (tables.Count == 0)
        {
            return new List<TableRowCountModel>();
        }

        await using var catalogContext = CreateCatalogContext(info);
        var query = BuildRowCountQuery(tables);
        return await catalogContext.Database
            .SqlQueryRaw<TableRowCountModel>(query.Sql, query.Parameters.ToArray())
            .ToListAsync(cancellationToken);
    }

    // islevi: Secili adreslerin mevcut tablo/kolon/PK yapisini provider katalog sorgusundan okur.
    public async Task<List<TableDataStructureModel>> ReadTableStructuresAsync(
        DatabaseConnectionInfo info,
        List<ComparisonTableIdentifierModel> tables,
        CancellationToken cancellationToken = default)
    {
        if (tables.Count == 0)
        {
            return new List<TableDataStructureModel>();
        }

        await using var catalogContext = CreateCatalogContext(info);
        return await ReadTableStructuresCoreAsync(catalogContext, tables, cancellationToken);
    }

    // islevi: Limit kontrolunden gecmis tablo yapilarinin tum satirlarini tek UNION ALL JSON sorgusuyla okur.
    public async Task<List<TableDataSnapshotModel>> ReadTableDataAsync(
        DatabaseConnectionInfo info,
        List<TableDataStructureModel> tables,
        CancellationToken cancellationToken = default)
    {
        if (tables.Count == 0)
        {
            return new List<TableDataSnapshotModel>();
        }

        await using var catalogContext = CreateCatalogContext(info);
        var query = BuildTableDataQuery(tables);
        var jsonRows = await catalogContext.Database
            .SqlQueryRaw<TableDataJsonRowModel>(query.Sql, query.Parameters.ToArray())
            .ToListAsync(cancellationToken);

        return BuildTableDataSnapshots(tables, jsonRows);
    }

    // islevi: Dogrulanmis tablo/anahtar icin JSON satirlarini provider LINQ limitine tabi tutarak okur.
    public async Task<List<TableDataRowModel>> ReadRowsByKeyAsync(
        DatabaseConnectionInfo info,
        TableDataStructureModel structure,
        Dictionary<string, string?> keyValues,
        int maxRows,
        CancellationToken cancellationToken = default)
    {
        await using var catalogContext = CreateCatalogContext(info);
        var query = BuildRowsByKeyQuery(structure, keyValues);
        var rows = await catalogContext.Database
            .SqlQueryRaw<TableDataJsonRowModel>(query.Sql, query.Parameters.ToArray())
            .Take(maxRows)
            .ToListAsync(cancellationToken);
        return rows.Select(row => ParseRow(row.RowJson)).ToList();
    }

    // islevi: Dogrulanmis tablo/anahtar icin kesin satir sayisini mevcut dinamik-SQL omurgasinda okur.
    public async Task<long> CountByKeyAsync(
        DatabaseConnectionInfo info,
        TableDataStructureModel structure,
        Dictionary<string, string?> keyValues,
        CancellationToken cancellationToken = default)
    {
        await using var catalogContext = CreateCatalogContext(info);
        var query = BuildCountByKeyQuery(structure, keyValues);
        return await catalogContext.Database
            .SqlQueryRaw<long>(query.Sql, query.Parameters.ToArray())
            .SingleAsync(cancellationToken);
    }

    // islevi: Katalogda dogrulanmis tablo, anahtar ve secili kolonlarla sinirli projection satirlarini okur.
    public virtual async Task<List<ProjectionRow>> ReadProjectionRowsAsync(
        DatabaseConnectionInfo info,
        TableDataStructureModel structure,
        Dictionary<string, string?> keyValues,
        List<string> projectColumns,
        int maxRows,
        CancellationToken cancellationToken = default)
    {
        await using var catalogContext = CreateCatalogContext(info);
        var query = BuildProjectionQuery(structure, keyValues, projectColumns);
        var rows = await catalogContext.Database
            .SqlQueryRaw<TableDataJsonRowModel>(query.Sql, query.Parameters.ToArray())
            .Take(maxRows)
            .ToListAsync(cancellationToken);
        return rows.Select(row => new ProjectionRow(ParseRow(row.RowJson).Values)).ToList();
    }

    // islevi: Projection adresi, kolonlari ve bagli anahtar parametrelerini tek salt-okunur query degerinde kurar.
    internal DynamicSqlQuery BuildProjectionQuery(
        TableDataStructureModel structure,
        Dictionary<string, string?> keyValues,
        List<string> projectColumns)
    {
        EnsureProjectColumnsExist(structure, projectColumns);
        var parameters = new List<object>();
        var predicate = BuildKeyPredicate(structure, keyValues, parameters);
        var rowAlias = "projection_row";
        var expression = BuildProjectedRowJsonExpression(rowAlias, projectColumns, parameters);
        var sql = BuildMetadataSelect(
            structure.SchemaName,
            structure.TableName,
            expression,
            nameof(TableDataJsonRowModel.RowJson),
            rowAlias,
            predicate,
            parameters);
        return new DynamicSqlQuery(sql, parameters);
    }

    // islevi: Anahtarli satir okumasini sema+tablo+JSON seklinde composable parametreli sorguya cevirir.
    private DynamicSqlQuery BuildRowsByKeyQuery(
        TableDataStructureModel structure,
        Dictionary<string, string?> keyValues)
    {
        var parameters = new List<object>();
        var predicate = BuildKeyPredicate(structure, keyValues, parameters);
        var rowAlias = "assertion_row";
        var sql = BuildMetadataSelect(
            structure.SchemaName,
            structure.TableName,
            BuildRowJsonExpression(rowAlias),
            nameof(TableDataJsonRowModel.RowJson),
            rowAlias,
            predicate,
            parameters);
        return new DynamicSqlQuery(sql, parameters);
    }

    // islevi: Anahtarli kesin sayimi EF scalar sonucunun Value alias'iyla composable sorguya cevirir.
    private DynamicSqlQuery BuildCountByKeyQuery(
        TableDataStructureModel structure,
        Dictionary<string, string?> keyValues)
    {
        var parameters = new List<object>();
        var predicate = BuildKeyPredicate(structure, keyValues, parameters);
        var sql = BuildScalarSelect(
            CountExpression,
            "Value",
            structure.SchemaName,
            structure.TableName,
            predicate);
        return new DynamicSqlQuery(sql, parameters);
    }

    // islevi: Katalogda dogrulanmis anahtar kolonlarini parametreli esitlik veya SQL NULL kosullarina cevirir.
    private string BuildKeyPredicate(
        TableDataStructureModel structure,
        Dictionary<string, string?> keyValues,
        List<object> parameters)
    {
        EnsureKeyColumnsExist(structure, keyValues.Keys);
        return string.Join(" and ", keyValues
            .OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
            .Select(pair => BuildKeyCondition(structure, pair.Key, pair.Value, parameters)));
    }

    // islevi: Tek anahtar degerini null-safe ve parametreli SQL kosuluna cevirir.
    private string BuildKeyCondition(
        TableDataStructureModel structure,
        string columnName,
        string? value,
        List<object> parameters)
    {
        var quotedColumn = QuoteIdentifier(columnName);
        if (value is null)
        {
            return $"{quotedColumn} is null";
        }

        var placeholder = BindParameter(parameters, ParseKeyParameter(structure, columnName, value));
        return $"{quotedColumn} = {placeholder}";
    }

    // islevi: Anahtar metnini katalogdaki kanonik tip ailesinin uygun CLR parametre cozumleyicisine yonlendirir.
    private static object ParseKeyParameter(
        TableDataStructureModel structure,
        string columnName,
        string value)
    {
        var typeCode = structure.Columns.First(column =>
            string.Equals(column.Name, columnName, StringComparison.OrdinalIgnoreCase)).CanonicalDataTypeCode;
        return typeCode switch
        {
            CanonicalDataTypeCodes.Integer or CanonicalDataTypeCodes.SmallInteger
                or CanonicalDataTypeCodes.BigInteger => ParseIntegerKey(value),
            CanonicalDataTypeCodes.Decimal or CanonicalDataTypeCodes.Money => ParseDecimalKey(value),
            CanonicalDataTypeCodes.Float or CanonicalDataTypeCodes.Double => ParseFloatingPointKey(value),
            CanonicalDataTypeCodes.Boolean => ParseBooleanKey(value),
            CanonicalDataTypeCodes.Uuid => ParseUuidKey(value),
            CanonicalDataTypeCodes.Date => ParseDateKey(value),
            CanonicalDataTypeCodes.Time => ParseTimeKey(value),
            CanonicalDataTypeCodes.Timestamp => ParseTimestampKey(value),
            CanonicalDataTypeCodes.TimestampWithTimeZone => ParseTimestampWithTimeZoneKey(value),
            _ => value
        };
    }

    // islevi: Integer ailesi anahtarini provider'larin sayisal esitlikte kabul ettigi Int64 parametresine cevirir.
    private static long ParseIntegerKey(string value)
        => long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : throw InvalidKeyValue();

    // islevi: Decimal/money anahtarini invariant Decimal parametresine cevirir.
    private static decimal ParseDecimalKey(string value)
        => decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : throw InvalidKeyValue();

    // islevi: Floating-point anahtarini invariant Double parametresine cevirir.
    private static double ParseFloatingPointKey(string value)
        => double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : throw InvalidKeyValue();

    // islevi: Boolean anahtarini kararli true/false CLR parametresine cevirir.
    private static bool ParseBooleanKey(string value)
        => bool.TryParse(value, out var parsed) ? parsed : throw InvalidKeyValue();

    // islevi: UUID anahtarini Guid parametresine cevirir.
    private static Guid ParseUuidKey(string value)
        => Guid.TryParse(value, out var parsed) ? parsed : throw InvalidKeyValue();

    // islevi: Date anahtarini provider'larin date parametresi olarak esledigi DateOnly degerine cevirir.
    private static DateOnly ParseDateKey(string value)
        => DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
            ? parsed
            : throw InvalidKeyValue();

    // islevi: Time anahtarini gun-ici TimeSpan parametresine cevirir.
    private static TimeSpan ParseTimeKey(string value)
        => TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : throw InvalidKeyValue();

    // islevi: Timezone'suz timestamp anahtarini Unspecified DateTime parametresine cevirir.
    private static DateTime ParseTimestampKey(string value)
    {
        var parsed = DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var timestamp)
            ? timestamp
            : throw InvalidKeyValue();
        return DateTime.SpecifyKind(parsed, DateTimeKind.Unspecified);
    }

    // islevi: Timezone'lu timestamp anahtarini UTC DateTimeOffset parametresine cevirir.
    private static DateTimeOffset ParseTimestampWithTimeZoneKey(string value)
        => DateTimeOffset.TryParse(
            value,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
            out var parsed)
            ? parsed
            : throw InvalidKeyValue();

    // islevi: Tip-semantik anahtar donusumu basarisizligini kararli assertion is hatasina cevirir.
    private static BusinessException InvalidKeyValue()
        => new(AssertionExceptionCodes.InvalidExpectedValue);

    // islevi: Dinamik identifier yuzeyine yalniz katalogdan gelmis kolon adlarinin girmesini garanti eder.
    private static void EnsureKeyColumnsExist(TableDataStructureModel structure, IEnumerable<string> keyColumns)
    {
        var columns = structure.ColumnNames.ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (keyColumns.Any(column => !columns.Contains(column)))
        {
            throw new BusinessException(AssertionExceptionCodes.InvalidExpectedValue);
        }
    }

    // islevi: Dinamik SELECT listesine yalniz katalogda bulunan ve en az bir secili kolonun girmesini garanti eder.
    private static void EnsureProjectColumnsExist(
        TableDataStructureModel structure,
        IReadOnlyCollection<string> projectColumns)
    {
        var columns = structure.ColumnNames.ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (projectColumns.Count == 0 || projectColumns.Any(column => !columns.Contains(column)))
        {
            throw new BusinessException(AssertionExceptionCodes.InvalidExpectedValue);
        }
    }

    // islevi: Dogrulanmis sema ve tabloyu provider quoting ile tam tablo adresine cevirir.
    private string BuildQualifiedName(string schemaName, string tableName)
        => $"{QuoteIdentifier(schemaName)}.{QuoteIdentifier(tableName)}";

    // Calisma-anindaki tablo icin derleme-zamani entity olmadigindan LINQ kurulamaz; dinamik SQL yalniz bu yardimci sette, katalogda dogrulanmis/quote edilmis identifier ve EF parametre baglama ile uretilir.
    // islevi: Secili tablolari tek UNION ALL sorgusunda kesin sayimla okuyan parametreli SQL'i uretir.
    private DynamicSqlQuery BuildRowCountQuery(List<ComparisonTableIdentifierModel> tables)
    {
        var selects = new List<string>(tables.Count);
        var parameters = new List<object>(tables.Count * 2);
        foreach (var table in tables)
        {
            selects.Add(BuildMetadataSelect(
                table.SchemaName,
                table.TableName,
                CountExpression,
                nameof(TableRowCountModel.RowCount),
                null,
                null,
                parameters));
        }
        return CombineSelects(selects, parameters);
    }

    // islevi: Degisken kolonlu secili tablolarin satirlarini sabit sema+tablo+JSON sekline paketleyen tek batch SQL'i kurar.
    private DynamicSqlQuery BuildTableDataQuery(List<TableDataStructureModel> tables)
    {
        var rowAlias = "data_row";
        var selects = new List<string>(tables.Count);
        var parameters = new List<object>(tables.Count * 2);
        foreach (var table in tables)
        {
            selects.Add(BuildMetadataSelect(
                table.SchemaName,
                table.TableName,
                BuildRowJsonExpression(rowAlias),
                nameof(TableDataJsonRowModel.RowJson),
                rowAlias,
                null,
                parameters));
        }
        return CombineSelects(selects, parameters);
    }

    // islevi: Sema/tablo metadata kolonlariyla deger kolonunu ayni guvenli SELECT iskeletinde kurar.
    private string BuildMetadataSelect(
        string schemaName,
        string tableName,
        string valueExpression,
        string valueAlias,
        string? rowAlias,
        string? predicate,
        List<object> parameters)
    {
        var schemaParameter = BindParameter(parameters, schemaName);
        var tableParameter = BindParameter(parameters, tableName);
        var from = BuildFromClause(schemaName, tableName, rowAlias);
        var where = BuildWhereClause(predicate);
        return $"select {schemaParameter} as {QuoteIdentifier(nameof(TableDataJsonRowModel.SchemaName))}, " +
               $"{tableParameter} as {QuoteIdentifier(nameof(TableDataJsonRowModel.TableName))}, " +
               $"{valueExpression} as {QuoteIdentifier(valueAlias)} from {from}{where}";
    }

    // islevi: Count-by-key gibi tek degerli sorgulari ayni qualified-name ve predicate yardimcilariyla kurar.
    private string BuildScalarSelect(
        string valueExpression,
        string valueAlias,
        string schemaName,
        string tableName,
        string? predicate)
        => $"select {valueExpression} as {QuoteIdentifier(valueAlias)} " +
           $"from {BuildQualifiedName(schemaName, tableName)}{BuildWhereClause(predicate)}";

    // islevi: Qualified tablo adina opsiyonel ve yalnizca kurucu tarafindan belirlenen row alias'ini ekler.
    private string BuildFromClause(string schemaName, string tableName, string? rowAlias)
        => rowAlias is null
            ? BuildQualifiedName(schemaName, tableName)
            : $"{BuildQualifiedName(schemaName, tableName)} as {rowAlias}";

    // islevi: Opsiyonel anahtar predicate'ini SELECT iskeletinin where parcasina cevirir.
    private static string BuildWhereClause(string? predicate)
        => predicate is null ? string.Empty : $" where {predicate}";

    // islevi: Tek degeri EF SqlQueryRaw placeholder'ina baglar ve placeholder'i dondurur.
    protected static string BindParameter(List<object> parameters, object value)
    {
        var placeholder = "{" + parameters.Count + "}";
        parameters.Add(value);
        return placeholder;
    }

    // islevi: Batch SELECT parcalarini tek query modelinde UNION ALL ile birlestirir.
    private static DynamicSqlQuery CombineSelects(List<string> selects, List<object> parameters)
        => new(string.Join(DatabaseMetadataCatalogConstants.UnionAllSeparator, selects), parameters);

    // islevi: JSON query satirlarini tablo bazinda gruplar ve provider-notr kanonik deger sozluklerine cevirir.
    private static List<TableDataSnapshotModel> BuildTableDataSnapshots(
        List<TableDataStructureModel> tables,
        List<TableDataJsonRowModel> jsonRows)
    {
        var snapshots = tables.ToDictionary(
            table => BuildTableKey(table.SchemaName, table.TableName),
            table => new TableDataSnapshotModel
            {
                SchemaName = table.SchemaName,
                TableName = table.TableName,
                ColumnNames = table.ColumnNames.ToList(),
                PrimaryKeyColumns = table.PrimaryKeyColumns.ToList()
            },
            StringComparer.OrdinalIgnoreCase);

        foreach (var jsonRow in jsonRows)
        {
            var key = BuildTableKey(jsonRow.SchemaName, jsonRow.TableName);
            if (snapshots.TryGetValue(key, out var snapshot))
            {
                snapshot.Rows.Add(ParseRow(jsonRow.RowJson));
            }
        }

        foreach (var snapshot in snapshots.Values)
        {
            snapshot.RowCount = snapshot.Rows.Count;
        }

        return snapshots.Values
            .OrderBy(snapshot => snapshot.SchemaName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(snapshot => snapshot.TableName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    // islevi: Provider JSON nesnesini case-insensitive kolon sozlugune ve kanonik scalar metinlere cevirir.
    private static TableDataRowModel ParseRow(string rowJson)
    {
        using var document = JsonDocument.Parse(rowJson);
        var row = new TableDataRowModel();
        foreach (var property in document.RootElement.EnumerateObject())
        {
            row.Values[property.Name] = NormalizeJsonValue(property.Value);
        }

        return row;
    }

    // islevi: JSON scalar/nested degerini motorlar arasi karsilastirilabilir kararli metne cevirir; DB NULL null kalir.
    private static string? NormalizeJsonValue(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Null || value.ValueKind == JsonValueKind.Undefined)
        {
            return null;
        }

        if (value.ValueKind == JsonValueKind.String)
        {
            return value.GetString();
        }

        if (value.ValueKind == JsonValueKind.Number)
        {
            var rawNumber = value.GetRawText();
            return decimal.TryParse(rawNumber, NumberStyles.Float, CultureInfo.InvariantCulture, out var decimalValue)
                ? decimalValue.ToString("G29", CultureInfo.InvariantCulture)
                : rawNumber;
        }

        if (value.ValueKind == JsonValueKind.True)
        {
            return bool.TrueString.ToLowerInvariant();
        }

        if (value.ValueKind == JsonValueKind.False)
        {
            return bool.FalseString.ToLowerInvariant();
        }

        return value.GetRawText();
    }

    // islevi: Sema+tablo adresini case-insensitive dictionary anahtarina cevirir.
    protected static string BuildTableKey(string schemaName, string tableName)
        => string.Join(ComparisonCanonicalTextConstants.KeySeparator, schemaName.Trim(), tableName.Trim());
}
