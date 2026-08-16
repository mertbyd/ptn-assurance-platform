using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Interface.Comparison;
using Ptn.DatabaseChecker.Models.Comparison;

namespace Ptn.DatabaseChecker.Repository.Comparison;

// islevi: Motor-ozel sema okuyucularinin ortak "snapshot oku" akis altyapisini saglar.
// sistemdeki gorevi: PostgreSQL/SQL Server repository'lerinde ayni context->schema->table->column->snapshot kurulumunu kopyalamadan, yalniz katalog LINQ farklarini abstract hook'lara birakir.
/// <summary>
/// Provider repository'lerinin paylastigi snapshot okuma iskeletidir.
/// Ortak akisi burada tutar; katalog tablo/kolon farklarini PostgreSQL ve SQL Server siniflarindaki hook'lara birakir.
/// </summary>
/// <typeparam name="TKey">
/// Katalog kimlik tipi. Provider'a gore oid/object_id/schema_id gibi anahtarlar farkli tipte oldugu icin generic tutulur.
/// </typeparam>
public abstract class DatabaseSchemaDiscoveryRepositoryBase<TKey> : IDatabaseSchemaDiscoveryRepository
    where TKey : struct
{
    // Motor koduna uygun tip esleme saglayicisini conventional DI koleksiyonundan secer.
    private readonly IEngineComponentResolver<IEngineTypeMapProvider>? _typeMapProviderResolver;

    // Ayni snapshot'taki tum kolonlar icin bir kez cozulup yeniden kullanilan motor tip saglayicisi.
    private IEngineTypeMapProvider? _typeMapProvider;

    // islevi: 0.1.x provider alt siniflarinin kullandigi parameterless base constructor'u ikili uyumlu tutar.
    protected DatabaseSchemaDiscoveryRepositoryBase()
    {
    }

    // islevi: Ortak snapshot akisina engine type-map resolver bagimliligini verir.
    protected DatabaseSchemaDiscoveryRepositoryBase(
        IEngineComponentResolver<IEngineTypeMapProvider> typeMapProviderResolver)
    {
        _typeMapProviderResolver = typeMapProviderResolver;
    }

    /// <summary>
    /// Bu okuyucunun destekledigi lookup engine kodu.
    /// </summary>
    public abstract string EngineCode { get; }

    /// <summary>
    /// Baglantidaki kullanici semalarini hafif listeler; ortak "context ac -> sema oku -> ada gore sirala" akisini burada tutar.
    /// Sema filtresi bos verilerek mevcut <see cref="GetSchemaNameByIdAsync"/> hook'u yeniden kullanilir; her motor kendi sistem semalarini eler.
    /// </summary>
    public async Task<List<DatabaseSchemaModel>> GetSchemasAsync(
        DatabaseConnectionInfo info,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = CreateDbContext(info);

        var schemaNameById = await GetSchemaNameByIdAsync(dbContext, new List<string>(), cancellationToken);

        return schemaNameById.Values
            .OrderBy(name => name)
            .Select(name => new DatabaseSchemaModel { Name = name })
            .ToList();
    }

    /// <summary>
    /// Verilen semadaki nesneleri hafif listeler; ortak "context ac -> nesne oku -> tur+ad'a gore sirala" akisini burada tutar.
    /// Native katalog okumasini <see cref="ReadObjectsAsync"/> hook'una birakir.
    /// </summary>
    public async Task<List<DatabaseSchemaObjectModel>> GetObjectsAsync(
        DatabaseConnectionInfo info,
        string schemaName,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = CreateDbContext(info);

        var objects = await ReadObjectsAsync(dbContext, schemaName, cancellationToken);

        return objects
            .OrderBy(schemaObject => schemaObject.ObjectTypeCode)
            .ThenBy(schemaObject => schemaObject.Name)
            .ToList();
    }

    /// <summary>
    /// Verilen baglanti ve sema filtresiyle tam schema snapshot'i uretir.
    /// </summary>
    public async Task<SchemaSnapshotModel> ReadSnapshotAsync(
        DatabaseConnectionInfo info,
        List<string> schemaNames,
        CancellationToken cancellationToken = default)
        => await ReadSnapshotCoreAsync(info, schemaNames, null, cancellationToken);

    /// <summary>
    /// Verilen semadaki tek tablonun katalog satirlarini mevcut snapshot sorgularina filtre uygulayarak okur.
    /// </summary>
    public async Task<SchemaSnapshotModel> ReadSnapshotAsync(
        DatabaseConnectionInfo info,
        List<string> schemaNames,
        string tableName,
        CancellationToken cancellationToken = default)
        => await ReadSnapshotCoreAsync(info, schemaNames, tableName, cancellationToken);

    // islevi: Tam sema veya tek tablo snapshot'ini ayni katalog okuma adimlariyla uretir.
    private async Task<SchemaSnapshotModel> ReadSnapshotCoreAsync(
        DatabaseConnectionInfo info,
        List<string> schemaNames,
        string? tableName,
        CancellationToken cancellationToken)
    {
        await using var dbContext = CreateDbContext(info);

        // Snapshot basligi once olusur; erken donuslerde bile engine/database bilgisi kaybolmaz.
        var snapshot = CreateSnapshot(info);
        await PopulateSnapshotMetadataAsync(dbContext, snapshot, cancellationToken);

        // 1) Sema secimi tek batch sorgudur: istek bos ise provider kendi sistem semalarini eler.
        var schemaNameById = await GetSchemaNameByIdAsync(dbContext, schemaNames, cancellationToken);
        if (schemaNameById.Count == 0)
        {
            return snapshot;
        }

        // 2) Tablo disi nesneler tek batch ailesiyle okunur; tablo olmasa bile snapshot dolu donebilir.
        if (tableName is null)
        {
            snapshot.Objects = await GetObjectDefinitionsAsync(dbContext, schemaNameById, cancellationToken);
        }

        // 3) Tablo secimi tek batch sorgudur; tablo kimlikleri kolon sorgusunun input'u olur.
        var tables = await GetTablesAsync(
            dbContext, schemaNameById.Keys.ToList(), tableName, cancellationToken);
        if (tables.Count == 0)
        {
            return snapshot;
        }

        var detailMaps = await ReadTableDetailMapsAsync(dbContext, tables, cancellationToken);

        snapshot.Tables = BuildSnapshotTables(
            tables,
            schemaNameById,
            detailMaps.ColumnsByTable,
            detailMaps.IndexesByTable,
            detailMaps.ConstraintsByTable,
            detailMaps.TriggersByTable);

        return snapshot;
    }

    // islevi: Runtime baglanti bilgisinden provider'a ozel migration-disi katalog DbContext'i kurar.
    /// <summary>
    /// Hedef veritabanina baglanan migration-disi catalog DbContext'i olusturur.
    /// </summary>
    protected abstract DbContext CreateDbContext(DatabaseConnectionInfo info);

    // islevi: Provider katalogundan veritabani-seviyesi collation metadata'sini snapshot basligina tasir.
    protected virtual Task PopulateSnapshotMetadataAsync(
        DbContext dbContext,
        SchemaSnapshotModel snapshot,
        CancellationToken cancellationToken)
        => Task.CompletedTask;

    // islevi: Verilen semadaki nesneleri (tablo/view/trigger/function/procedure) native katalogdan okuyup motor-bagimsiz nesne modellerine cevirir.
    /// <summary>
    /// Verilen semadaki nesneleri provider katalogundan okur; sema yoksa bos liste doner.
    /// </summary>
    protected abstract Task<List<DatabaseSchemaObjectModel>> ReadObjectsAsync(
        DbContext dbContext,
        string schemaName,
        CancellationToken cancellationToken);

    // islevi: Secilen semalardaki tablo disi nesne tanimlarini ortak tur sirasiyla okuyup tek kararli listeye toplar.
    /// <summary>
    /// Secilen semalardaki tablo disi nesne tanimlarini provider hook'larindan toplar.
    /// </summary>
    protected virtual async Task<List<SchemaObjectDefinitionModel>> GetObjectDefinitionsAsync(
        DbContext dbContext,
        Dictionary<TKey, string> schemaNameById,
        CancellationToken cancellationToken)
    {
        var objects = new List<SchemaObjectDefinitionModel>();

        objects.AddRange(await ReadViewDefinitionsAsync(dbContext, schemaNameById, cancellationToken));
        objects.AddRange(await ReadRoutineDefinitionsAsync(dbContext, schemaNameById, cancellationToken));
        objects.AddRange(await ReadSequenceDefinitionsAsync(dbContext, schemaNameById, cancellationToken));
        objects.AddRange(await ReadTypeDefinitionsAsync(dbContext, schemaNameById, cancellationToken));
        objects.AddRange(await ReadExtensionDefinitionsAsync(dbContext, schemaNameById, cancellationToken));

        return SortObjectDefinitions(objects);
    }

    // islevi: Provider destekliyorsa view/materialized-view tanimlarini okur; desteklemeyen motorlar bos hook ile akar.
    /// <summary>
    /// Secilen semalardaki view tanimlarini okur.
    /// </summary>
    protected virtual Task<List<SchemaObjectDefinitionModel>> ReadViewDefinitionsAsync(
        DbContext dbContext,
        Dictionary<TKey, string> schemaNameById,
        CancellationToken cancellationToken)
        => CreateEmptyObjectDefinitionsTask();

    // islevi: Provider destekliyorsa function/procedure tanimlarini okur; desteklemeyen motorlar bos hook ile akar.
    /// <summary>
    /// Secilen semalardaki routine tanimlarini okur.
    /// </summary>
    protected virtual Task<List<SchemaObjectDefinitionModel>> ReadRoutineDefinitionsAsync(
        DbContext dbContext,
        Dictionary<TKey, string> schemaNameById,
        CancellationToken cancellationToken)
        => CreateEmptyObjectDefinitionsTask();

    // islevi: Provider destekliyorsa sequence tanimlarini okur; desteklemeyen motorlar bos hook ile akar.
    /// <summary>
    /// Secilen semalardaki sequence tanimlarini okur.
    /// </summary>
    protected virtual Task<List<SchemaObjectDefinitionModel>> ReadSequenceDefinitionsAsync(
        DbContext dbContext,
        Dictionary<TKey, string> schemaNameById,
        CancellationToken cancellationToken)
        => CreateEmptyObjectDefinitionsTask();

    // islevi: Provider destekliyorsa enum/domain/user-defined type tanimlarini okur; desteklemeyen motorlar bos hook ile akar.
    /// <summary>
    /// Secilen semalardaki type tanimlarini okur.
    /// </summary>
    protected virtual Task<List<SchemaObjectDefinitionModel>> ReadTypeDefinitionsAsync(
        DbContext dbContext,
        Dictionary<TKey, string> schemaNameById,
        CancellationToken cancellationToken)
        => CreateEmptyObjectDefinitionsTask();

    // islevi: Provider destekliyorsa extension/plugin tanimlarini okur; desteklemeyen motorlar bos hook ile akar.
    /// <summary>
    /// Secilen semalardaki extension tanimlarini okur.
    /// </summary>
    protected virtual Task<List<SchemaObjectDefinitionModel>> ReadExtensionDefinitionsAsync(
        DbContext dbContext,
        Dictionary<TKey, string> schemaNameById,
        CancellationToken cancellationToken)
        => CreateEmptyObjectDefinitionsTask();

    // islevi: Okunacak semalari katalog kimligi -> sema adi sozlugu olarak tek batch sorguda dondurur.
    /// <summary>
    /// Okunacak semalari katalog kimligi uzerinden ad sozlugune cevirir.
    /// </summary>
    protected abstract Task<Dictionary<TKey, string>> GetSchemaNameByIdAsync(
        DbContext dbContext,
        List<string> schemaNames,
        CancellationToken cancellationToken);

    // islevi: Secilen semalara ait kullanici tablolarini tek batch sorguda dondurur.
    /// <summary>
    /// Secilen semalara ait kullanici tablolarini provider katalogundan okur.
    /// </summary>
    protected abstract Task<List<SchemaDiscoveredTableModel<TKey>>> GetTablesAsync(
        DbContext dbContext,
        List<TKey> schemaIds,
        CancellationToken cancellationToken);

    // islevi: Hedefli describe icin tablo filtresini provider'in sunucu sorgusuna tasiyabilen opsiyonel hook'u saglar.
    protected virtual async Task<List<SchemaDiscoveredTableModel<TKey>>> GetTablesAsync(
        DbContext dbContext,
        List<TKey> schemaIds,
        string? tableName,
        CancellationToken cancellationToken)
    {
        var tables = await GetTablesAsync(dbContext, schemaIds, cancellationToken);
        return string.IsNullOrWhiteSpace(tableName)
            ? tables
            : tables.Where(table => string.Equals(table.Name, tableName, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    // islevi: Secilen tablolarin kolonlarini tablo kimligine gore gruplanmis kanonik kolon modelleri olarak dondurur.
    /// <summary>
    /// Secilen tablo kimlikleri icin kolonlari okur ve tablo kimligine gore gruplanmis halde dondurur.
    /// </summary>
    protected abstract Task<Dictionary<TKey, List<SchemaColumnModel>>> GetColumnsByTableAsync(
        DbContext dbContext,
        List<TKey> tableIds,
        CancellationToken cancellationToken);

    // islevi: Ham katalog tipini bir kez esleyip kanonik aile ve fidelity sonucunu kolon modeline tasir.
    protected SchemaColumnModel ApplyTypeMapping(SchemaColumnModel column, string rawTypeName)
    {
        if (_typeMapProviderResolver is null)
        {
            return column;
        }

        var typeMapProvider = _typeMapProvider ??= _typeMapProviderResolver.Resolve(EngineCode);
        if (typeMapProvider.TryMap(rawTypeName, out var mapping))
        {
            column.CanonicalDataType = mapping.CanonicalTypeCode;
            column.TypeMappingFidelityCode = mapping.FidelityCode;
        }

        return column;
    }

    // islevi: Secilen tablolarin index'lerini tablo kimligine gore gruplanmis kanonik index modelleri olarak dondurur.
    /// <summary>
    /// Secilen tablo kimlikleri icin index'leri okur ve tablo kimligine gore gruplanmis halde dondurur.
    /// </summary>
    protected abstract Task<Dictionary<TKey, List<SchemaIndexModel>>> GetIndexesByTableAsync(
        DbContext dbContext,
        List<TKey> tableIds,
        Dictionary<TKey, List<SchemaColumnModel>> columnsByTable,
        CancellationToken cancellationToken);

    // islevi: Secilen tablolarin constraint'lerini tablo kimligine gore gruplanmis kanonik constraint modelleri olarak dondurur.
    /// <summary>
    /// Secilen tablolar icin PK/unique/FK/check constraint'lerini okur ve tablo kimligine gore gruplanmis halde dondurur.
    /// </summary>
    protected abstract Task<Dictionary<TKey, List<SchemaConstraintModel>>> GetConstraintsByTableAsync(
        DbContext dbContext,
        List<SchemaDiscoveredTableModel<TKey>> tables,
        Dictionary<TKey, List<SchemaColumnModel>> columnsByTable,
        CancellationToken cancellationToken);

    // islevi: Secilen tablolarin trigger'larini tablo kimligine gore gruplanmis kanonik trigger modelleri olarak dondurur.
    /// <summary>
    /// Secilen tablo kimlikleri icin trigger'lari okur ve tablo kimligine gore gruplanmis halde dondurur.
    /// </summary>
    protected abstract Task<Dictionary<TKey, List<SchemaTriggerModel>>> GetTriggersByTableAsync(
        DbContext dbContext,
        List<TKey> tableIds,
        CancellationToken cancellationToken);

    // islevi: Fotografin sabit kimlik alanlarini (motor kodu, veritabani adi, okuma ani) tasiyan bos snapshot uretir.
    private SchemaSnapshotModel CreateSnapshot(DatabaseConnectionInfo info)
        => new()
        {
            EngineCode = EngineCode,
            DatabaseName = info.DatabaseName,
            CollectedAt = DateTime.UtcNow
        };

    // islevi: Tablo alt detaylarini turlerine gore batch okuyup tablo kimligi sozluklerine ayirir.
    private async Task<SchemaTableDetailMaps<TKey>> ReadTableDetailMapsAsync(
        DbContext dbContext,
        List<SchemaDiscoveredTableModel<TKey>> tables,
        CancellationToken cancellationToken)
    {
        var tableIds = tables.Select(table => table.Id).ToList();

        // Kolonlar once okunur; index/constraint mapping'i kolon numarasi -> kolon adi sozlugunu kullanir.
        var columnsByTable = await GetColumnsByTableAsync(dbContext, tableIds, cancellationToken);
        var indexesByTable = await GetIndexesByTableAsync(dbContext, tableIds, columnsByTable, cancellationToken);
        var constraintsByTable = await GetConstraintsByTableAsync(
            dbContext,
            tables,
            columnsByTable,
            cancellationToken);
        var triggersByTable = await GetTriggersByTableAsync(dbContext, tableIds, cancellationToken);

        return new SchemaTableDetailMaps<TKey>
        {
            ColumnsByTable = columnsByTable,
            IndexesByTable = indexesByTable,
            ConstraintsByTable = constraintsByTable,
            TriggersByTable = triggersByTable
        };
    }

    // islevi: Provider'dan gelen tablo ve alt-detay sozluklerini motor bagimsiz snapshot tablo modellerine cevirir.
    private static List<SchemaTableModel> BuildSnapshotTables(
        List<SchemaDiscoveredTableModel<TKey>> tables,
        Dictionary<TKey, string> schemaNameById,
        Dictionary<TKey, List<SchemaColumnModel>> columnsByTable,
        Dictionary<TKey, List<SchemaIndexModel>> indexesByTable,
        Dictionary<TKey, List<SchemaConstraintModel>> constraintsByTable,
        Dictionary<TKey, List<SchemaTriggerModel>> triggersByTable)
    {
        return tables
            .Select(table => new SchemaTableModel
            {
                Schema = schemaNameById[table.SchemaId],
                Name = table.Name,
                Columns = columnsByTable.GetValueOrDefault(table.Id) ?? new List<SchemaColumnModel>(),
                Indexes = indexesByTable.GetValueOrDefault(table.Id) ?? new List<SchemaIndexModel>(),
                Constraints = constraintsByTable.GetValueOrDefault(table.Id) ?? new List<SchemaConstraintModel>(),
                Triggers = triggersByTable.GetValueOrDefault(table.Id) ?? new List<SchemaTriggerModel>()
            })
            .OrderBy(table => table.Schema)
            .ThenBy(table => table.Name)
            .ToList();
    }

    // islevi: Snapshot nesne listesinin provider sorgu sirasindan etkilenmeden kararli donmesini saglar.
    protected static List<SchemaObjectDefinitionModel> SortObjectDefinitions(List<SchemaObjectDefinitionModel> objects)
        => objects
            .OrderBy(schemaObject => schemaObject.Schema)
            .ThenBy(schemaObject => schemaObject.ObjectTypeCode)
            .ThenBy(schemaObject => schemaObject.Name)
            .ToList();

    // islevi: Desteklenmeyen tablo-disi nesne hook'larinda paylasimli mutable liste dondurmeden bos sonuc uretir.
    private static Task<List<SchemaObjectDefinitionModel>> CreateEmptyObjectDefinitionsTask()
        => Task.FromResult(new List<SchemaObjectDefinitionModel>());

    // islevi: Verilen alanlardan "varchar(100)", "numeric(10,2)", "nvarchar(max)" gibi ham tip metni uretir.
    /// <summary>
    /// Provider'larin cozumledigi uzunluk/precision/scale bilgisini raporda gosterilecek ham tip metnine cevirir.
    /// </summary>
    protected static string BuildRawDataType(string typeName, int? maxLength, int? precision, int? scale, bool isMaxLength = false)
    {
        if (isMaxLength)
        {
            return $"{typeName}(max)";
        }

        if (maxLength.HasValue)
        {
            return $"{typeName}({maxLength.Value})";
        }

        if (precision.HasValue)
        {
            return scale is > 0
                ? $"{typeName}({precision.Value},{scale.Value})"
                : $"{typeName}({precision.Value})";
        }

        return typeName;
    }

    // islevi: Provider'lardan gelen sequence ayarlarini ayni raporlanabilir tanim formatina cevirir.
    protected static string BuildSequenceDefinition(
        string dataType,
        string startValue,
        string minimumValue,
        string maximumValue,
        string increment,
        string cycleOption,
        string? cacheValue = null)
    {
        var parts = new List<string>
        {
            SchemaComparisonTextConstants.DefinitionTokens.Sequence,
            SchemaComparisonTextConstants.DefinitionTokens.As,
            dataType,
            SchemaComparisonTextConstants.DefinitionTokens.Start,
            startValue,
            SchemaComparisonTextConstants.DefinitionTokens.Minimum,
            minimumValue,
            SchemaComparisonTextConstants.DefinitionTokens.Maximum,
            maximumValue,
            SchemaComparisonTextConstants.DefinitionTokens.Increment,
            increment,
            SchemaComparisonTextConstants.DefinitionTokens.Cycle,
            cycleOption
        };

        if (!string.IsNullOrWhiteSpace(cacheValue))
        {
            parts.Add(SchemaComparisonTextConstants.DefinitionTokens.Cache);
            parts.Add(cacheValue);
        }

        return string.Join(SchemaComparisonTextConstants.Normalization.SingleSpace, parts);
    }
}
