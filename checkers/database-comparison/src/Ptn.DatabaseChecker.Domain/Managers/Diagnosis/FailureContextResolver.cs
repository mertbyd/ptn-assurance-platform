using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Constants.Diagnosis;
using Ptn.DatabaseChecker.Entities.Connections;
using Ptn.DatabaseChecker.ExceptionCodes;
using Ptn.DatabaseChecker.Managers.Comparison;
using Ptn.DatabaseChecker.Managers.SchemaDiscovery;
using Ptn.DatabaseChecker.Models.Assertions;
using Ptn.DatabaseChecker.Models.Comparison;
using Ptn.DatabaseChecker.Models.Diagnosis;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace Ptn.DatabaseChecker.Managers.Diagnosis;

// islevi: Cikarilan hata adlarini canli snapshot'ta dogrular ve kurallarin kullanacagi katalog/anahtar olgularini cozer.
// sistemdeki gorevi: Dogrulanmamis provider adlarini atip kimlik guvenini dusuren kimlik -> katalog guvenlik siniridir.
public class FailureContextResolver : DomainService
{
    private readonly SchemaDiscoveryManager _schemaDiscoveryManager;
    private readonly DatabaseDataComparisonManager _dataComparisonManager;
    private readonly FindingValueRedactor _redactor;

    // islevi: Resolver'i mevcut schema discovery, KBP-704 veri yapisi okuyucusu ve tek redactor ile kurar.
    public FailureContextResolver(
        SchemaDiscoveryManager schemaDiscoveryManager,
        DatabaseDataComparisonManager dataComparisonManager,
        FindingValueRedactor redactor)
    {
        _schemaDiscoveryManager = schemaDiscoveryManager;
        _dataComparisonManager = dataComparisonManager;
        _redactor = redactor;
    }

    // islevi: Engine'i dogrular, snapshot'i okur, nesne adlarini kanitlar ve probe hedeflerini sirayla cozer.
    public virtual async Task<ResolvedFailureContext> ResolveAsync(
        DatabaseConnection connection,
        FailureSignal signal,
        FailureIdentity identity,
        ValueRetentionPolicy retentionPolicy,
        CancellationToken cancellationToken = default)
    {
        EnsureEngineMatches(connection, identity);
        var snapshot = await ReadSnapshotAsync(connection, signal, identity, cancellationToken);
        var table = FindTable(snapshot, signal, identity);
        var context = BuildContext(connection, signal, identity, table, retentionPolicy);
        ResolveCatalogObjects(context, identity);
        ResolveKeyFacts(context, snapshot, identity);
        await ResolveProbeStructuresAsync(connection, context, cancellationToken);
        return context;
    }

    // islevi: Sinyalin engine'i ile kayitli baglantinin engine'inin ayni oldugunu fail-closed dogrular.
    private static void EnsureEngineMatches(DatabaseConnection connection, FailureIdentity identity)
    {
        if (!string.Equals(connection.Engine.Code, identity.EngineCode, StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException(DiagnosisExceptionCodes.EngineMismatch);
        }
    }

    // islevi: Bilinen semayi dar filtreyle, bilinmeyen DB-exception adresini tum kullanici semalariyla canli okur.
    private Task<SchemaSnapshotModel> ReadSnapshotAsync(
        DatabaseConnection connection,
        FailureSignal signal,
        FailureIdentity identity,
        CancellationToken cancellationToken)
    {
        var schemaName = signal.Assertion?.SchemaName ?? identity.ObjectReferences.FirstOrDefault()?.SchemaName;
        var schemas = string.IsNullOrWhiteSpace(schemaName)
            ? new List<string>()
            : new List<string> { schemaName };
        return _schemaDiscoveryManager.ReadSnapshotAsync(connection, schemas, cancellationToken);
    }

    // islevi: Assertion adresini veya provider referansini snapshot'taki tek gercek tabloya yerellestirir.
    private static SchemaTableModel? FindTable(
        SchemaSnapshotModel snapshot,
        FailureSignal signal,
        FailureIdentity identity)
    {
        var reference = identity.ObjectReferences.FirstOrDefault();
        var schemaName = signal.Assertion?.SchemaName ?? reference?.SchemaName;
        var tableName = signal.Assertion?.TableName ?? reference?.TableName;
        var direct = snapshot.Tables.FirstOrDefault(table => MatchesTable(table, schemaName, tableName));
        return direct ?? FindTableByConstraint(snapshot, reference?.ConstraintName);
    }

    // islevi: Sema ve tablo adinin snapshot tablosuyla case-insensitive eslesip eslesmedigini bildirir.
    private static bool MatchesTable(SchemaTableModel table, string? schemaName, string? tableName)
        => !string.IsNullOrWhiteSpace(tableName) &&
           string.Equals(table.Name, tableName, StringComparison.OrdinalIgnoreCase) &&
           (string.IsNullOrWhiteSpace(schemaName) ||
            string.Equals(table.Schema, schemaName, StringComparison.OrdinalIgnoreCase));

    // islevi: Tablo adi gelmeyen PostgreSQL hatasini dogrulanmis constraint adi uzerinden tek tabloya indirger.
    private static SchemaTableModel? FindTableByConstraint(SchemaSnapshotModel snapshot, string? constraintName)
        => string.IsNullOrWhiteSpace(constraintName)
            ? null
            : snapshot.Tables.FirstOrDefault(table => table.Constraints.Any(constraint =>
                string.Equals(constraint.Name, constraintName, StringComparison.OrdinalIgnoreCase)));

    // islevi: Kimlik olgulari, redaction uygulanmis failure kaniti ve canli tabloyu temel context modelinde toplar.
    private ResolvedFailureContext BuildContext(
        DatabaseConnection connection,
        FailureSignal signal,
        FailureIdentity identity,
        SchemaTableModel? table,
        ValueRetentionPolicy retentionPolicy)
        => new()
        {
            EngineCode = connection.Engine.Code,
            Table = table,
            RowWasReportedMissing = identity.IndicatesMissingRow,
            RowTimedOut = identity.IndicatesTimedOut,
            ValueWasReportedDifferent = identity.IndicatesValueMismatch,
            TargetKeyValues = CopyKeys(signal.Assertion?.KeyValues),
            FailedExpectations = RedactFailures(signal.Assertion?.FailedExpectations, retentionPolicy),
            MissingExpectedColumns = FindMissingColumns(table, signal.Assertion?.FailedExpectations)
        };

    // islevi: Nullable anahtar sozlugunu case-insensitive yeni bir domain sozlugune kopyalar.
    private static Dictionary<string, string?> CopyKeys(Dictionary<string, string?>? values)
        => values is null
            ? new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string?>(values, StringComparer.OrdinalIgnoreCase);

    // islevi: Disaridan yeniden sunulan failed-expectation degerlerini mevcut retention politikasindan tekrar gecirir.
    private List<FailedExpectation> RedactFailures(
        List<FailedExpectation>? failures,
        ValueRetentionPolicy retentionPolicy)
        => failures?.Select(failure => new FailedExpectation
        {
            ColumnName = failure.ColumnName,
            MatcherKindCode = failure.MatcherKindCode,
            ExpectedValue = _redactor.Redact(failure.ExpectedValue, retentionPolicy),
            ObservedValue = _redactor.Redact(failure.ObservedValue, retentionPolicy)
        }).ToList() ?? new List<FailedExpectation>();

    // islevi: Assertion failure kolonlarindan canli katalogda bulunmayanlari case-insensitive listeler.
    private static List<string> FindMissingColumns(
        SchemaTableModel? table,
        List<FailedExpectation>? failures)
    {
        if (table is null || failures is null)
        {
            return new List<string>();
        }

        var columns = table.Columns.Select(column => column.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        return failures.Select(item => item.ColumnName).Where(name => !columns.Contains(name)).Distinct().ToList();
    }

    // islevi: Provider referanslarini tablo, kolon ve constraint kataloglarinda dogrular; basarisiz adlari kimlikten atar.
    private static void ResolveCatalogObjects(ResolvedFailureContext context, FailureIdentity identity)
    {
        var reference = identity.ObjectReferences.FirstOrDefault() ?? new ObjectReference();
        if (context.Table is null)
        {
            RejectReference(identity, reference);
            return;
        }

        context.Location = VerifyReference(context.Table, reference, identity);
        context.Column = ResolveColumn(context.Table, context.Location.ColumnName, context.FailedExpectations);
        context.Constraint = ResolveConstraint(context.Table, context.Location.ConstraintName, context.TargetKeyValues);
        context.UniqueIndex = ResolveUniqueIndex(context.Table, context.TargetKeyValues);
    }

    // islevi: Tablo bulunamadiginda tum provider adlarini atip kimlik guvenini dusurur.
    private static void RejectReference(FailureIdentity identity, ObjectReference reference)
    {
        reference.SchemaName = null;
        reference.TableName = null;
        reference.ColumnName = null;
        reference.ConstraintName = null;
        reference.IsCatalogVerified = false;
        identity.ObjectReferences.RemoveAll(item => !item.HasName());
        identity.Downgrade();
    }

    // islevi: Tabloyu kesin, opsiyonel kolon/constraint adlarini yalniz katalogda varsa konuma tasir.
    private static ObjectReference VerifyReference(
        SchemaTableModel table,
        ObjectReference reference,
        FailureIdentity identity)
    {
        VerifyTableAddress(table, reference, identity);
        var location = new ObjectReference
        {
            SchemaName = table.Schema,
            TableName = table.Name,
            ColumnName = VerifyColumnName(table, reference.ColumnName, identity),
            ConstraintName = VerifyConstraintName(table, reference.ConstraintName, identity),
            IsCatalogVerified = true
        };
        identity.ObjectReferences = new List<ObjectReference> { location };
        return location;
    }

    // islevi: Provider sema/tablo adi katalogda cozulmus gercek adresle celisiyorsa kimlik guvenini dusurur.
    private static void VerifyTableAddress(
        SchemaTableModel table,
        ObjectReference reference,
        FailureIdentity identity)
    {
        var schemaMismatch = !string.IsNullOrWhiteSpace(reference.SchemaName) &&
                             !string.Equals(reference.SchemaName, table.Schema, StringComparison.OrdinalIgnoreCase);
        var tableMismatch = !string.IsNullOrWhiteSpace(reference.TableName) &&
                            !string.Equals(reference.TableName, table.Name, StringComparison.OrdinalIgnoreCase);
        if (schemaMismatch || tableMismatch)
        {
            identity.Downgrade();
        }
    }

    // islevi: Provider kolon adini canli tabloda bulamazsa atip kimlik guvenini dusurur.
    private static string? VerifyColumnName(
        SchemaTableModel table,
        string? columnName,
        FailureIdentity identity)
    {
        if (string.IsNullOrWhiteSpace(columnName))
        {
            return null;
        }

        var column = table.Columns.FirstOrDefault(item =>
            string.Equals(item.Name, columnName, StringComparison.OrdinalIgnoreCase));
        if (column is not null)
        {
            return column.Name;
        }

        identity.Downgrade();
        return null;
    }

    // islevi: Provider constraint adini canli tabloda bulamazsa atip kimlik guvenini dusurur.
    private static string? VerifyConstraintName(
        SchemaTableModel table,
        string? constraintName,
        FailureIdentity identity)
    {
        if (string.IsNullOrWhiteSpace(constraintName))
        {
            return null;
        }

        var constraint = table.Constraints.FirstOrDefault(item =>
            string.Equals(item.Name, constraintName, StringComparison.OrdinalIgnoreCase));
        if (constraint is not null)
        {
            return constraint.Name;
        }

        identity.Downgrade();
        return null;
    }

    // islevi: Dogrulanmis kolon adini veya failure kolonlari arasindaki generated kolonu context'e secer.
    private static SchemaColumnModel? ResolveColumn(
        SchemaTableModel table,
        string? columnName,
        List<FailedExpectation> failures)
        => table.Columns.FirstOrDefault(column =>
               string.Equals(column.Name, columnName, StringComparison.OrdinalIgnoreCase))
           ?? table.Columns.FirstOrDefault(column => failures.Any(failure =>
               string.Equals(failure.ColumnName, column.Name, StringComparison.OrdinalIgnoreCase)));

    // islevi: Dogrulanmis ada oncelik verir; yoksa anahtar kolonlarini kapsayan FK/unique constraint olgusunu secer.
    private static SchemaConstraintModel? ResolveConstraint(
        SchemaTableModel table,
        string? constraintName,
        Dictionary<string, string?> keys)
        => table.Constraints.FirstOrDefault(constraint =>
               string.Equals(constraint.Name, constraintName, StringComparison.OrdinalIgnoreCase))
           ?? table.Constraints.FirstOrDefault(constraint =>
               constraint.Columns.Count > 0 && constraint.Columns.All(keys.ContainsKey));

    // islevi: Assertion anahtar kolonlarini kapsayan filtresiz unique/PK index'i canli katalogdan secer.
    private static SchemaIndexModel? ResolveUniqueIndex(
        SchemaTableModel table,
        Dictionary<string, string?> keys)
        => table.Indexes.FirstOrDefault(index =>
            (index.IsUnique || index.IsPrimaryKey) &&
            string.IsNullOrWhiteSpace(index.FilterDefinition) &&
            index.Columns.Count > 0 && index.Columns.All(keys.ContainsKey));

    // islevi: Kimlik, parent ve PostgreSQL search_path/collation olgularini dogrulanmis snapshot'tan turetir.
    private static void ResolveKeyFacts(
        ResolvedFailureContext context,
        SchemaSnapshotModel snapshot,
        FailureIdentity identity)
    {
        context.IdentityKeyValues = ResolveIdentityKeys(context.Table, context.TargetKeyValues);
        context.ParentKeyValues = ResolveParentKeys(context.Constraint, context.TargetKeyValues);
        if (identity.SupportsServerSettingProbe && context.Table is not null)
        {
            context.ServerSettingExpectations[PostgreSqlSqlStateCodes.SettingNames.SearchPath] = context.Table.Schema;
            AddCollationExpectation(context, snapshot.DatabaseCollationName);
        }
    }

    // islevi: Snapshot PostgreSQL database collation'i biliniyorsa izinli lc_collate beklentisine ekler.
    private static void AddCollationExpectation(
        ResolvedFailureContext context,
        string? databaseCollationName)
    {
        if (!string.IsNullOrWhiteSpace(databaseCollationName))
        {
            context.ServerSettingExpectations[PostgreSqlSqlStateCodes.SettingNames.Collation] =
                databaseCollationName;
        }
    }

    // islevi: Tam assertion anahtarindan katalogda unique olan en dar kolon kumesini degerleriyle secer.
    private static Dictionary<string, string?> ResolveIdentityKeys(
        SchemaTableModel? table,
        Dictionary<string, string?> targetKeys)
    {
        var keyColumns = GetUniqueColumnSets(table)
            .Where(columns => columns.All(targetKeys.ContainsKey))
            .OrderBy(columns => columns.Count)
            .FirstOrDefault();
        return keyColumns is null
            ? CopyKeys(targetKeys)
            : keyColumns.ToDictionary(column => column, column => targetKeys[column], StringComparer.OrdinalIgnoreCase);
    }

    // islevi: Tablonun filtresiz unique/PK index ve constraint kolon kumelerini tek sirali akista verir.
    private static IEnumerable<List<string>> GetUniqueColumnSets(SchemaTableModel? table)
    {
        if (table is null)
        {
            return Enumerable.Empty<List<string>>();
        }

        var indexes = table.Indexes.Where(index =>
            (index.IsUnique || index.IsPrimaryKey) && string.IsNullOrWhiteSpace(index.FilterDefinition));
        var constraints = table.Constraints.Where(constraint =>
            constraint.TypeCode is SchemaConstraintTypeCodes.PrimaryKey or SchemaConstraintTypeCodes.Unique);
        return indexes.Select(index => index.Columns).Concat(constraints.Select(item => item.Columns));
    }

    // islevi: FK local kolon degerlerini sirali referenced kolon adlarina cevirerek parent probe anahtarini kurar.
    private static Dictionary<string, string?> ResolveParentKeys(
        SchemaConstraintModel? constraint,
        Dictionary<string, string?> targetKeys)
    {
        var result = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        if (constraint?.TypeCode != SchemaConstraintTypeCodes.ForeignKey ||
            constraint.Columns.Count != constraint.ReferencedColumns.Count)
        {
            return result;
        }

        for (var index = 0; index < constraint.Columns.Count; index++)
        {
            if (targetKeys.TryGetValue(constraint.Columns[index], out var value))
            {
                result[constraint.ReferencedColumns[index]] = value;
            }
        }

        return result.Count == constraint.Columns.Count ? result : new Dictionary<string, string?>();
    }

    // islevi: Hedef ve varsa FK parent tablo yapilarini KBP-704 mevcut discovery metodundan cozer.
    private async Task ResolveProbeStructuresAsync(
        DatabaseConnection connection,
        ResolvedFailureContext context,
        CancellationToken cancellationToken)
    {
        if (context.Table is null)
        {
            return;
        }

        context.TargetStructure = await _dataComparisonManager.ResolveAssertionStructureAsync(
            connection, context.Table.Schema, context.Table.Name, cancellationToken);
        await ResolveParentStructureAsync(connection, context, cancellationToken);
    }

    // islevi: FK adresini schema/table'a ayirip parent RowExists probe'u icin mevcut katalog yapisini cozer.
    private async Task ResolveParentStructureAsync(
        DatabaseConnection connection,
        ResolvedFailureContext context,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(context.Constraint?.ReferencedTable))
        {
            return;
        }

        var (schemaName, tableName) = SplitTableAddress(context.Table!.Schema, context.Constraint.ReferencedTable);
        context.ParentStructure = await _dataComparisonManager.ResolveAssertionStructureAsync(
            connection, schemaName, tableName, cancellationToken);
    }

    // islevi: Provider FK adresini mevcut tablo semasi varsayimiyla schema ve tablo parcalarina ayirir.
    private static (string SchemaName, string TableName) SplitTableAddress(
        string defaultSchema,
        string address)
    {
        var separatorIndex = address.IndexOf('.');
        return separatorIndex < 0
            ? (defaultSchema, address)
            : (address[..separatorIndex], address[(separatorIndex + 1)..]);
    }
}
