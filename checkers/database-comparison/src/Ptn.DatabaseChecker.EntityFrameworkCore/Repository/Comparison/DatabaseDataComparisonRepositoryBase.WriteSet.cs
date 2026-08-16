using System.Globalization;
using System.Text.RegularExpressions;
using Npgsql;
using Npgsql.Replication;
using Npgsql.Replication.TestDecoding;
using Ptn.DatabaseChecker.Constants.Capabilities;
using Ptn.DatabaseChecker.Models.Capabilities;
using Ptn.DatabaseChecker.Models.Comparison;
using Ptn.DatabaseChecker.Models.Correlation;

namespace Ptn.DatabaseChecker.Repository.Comparison;

// islevi: Mevcut onayli repository base icinde write-set probe, temporary logical slot ve decoded footprint yardimcilarini toplar.
// sistemdeki gorevi: Npgsql baglanti/replication protokolunu Domain ve Application'dan uzak tutar; diger motorlar ayni interface'te Unavailable doner.
public abstract partial class DatabaseDataComparisonRepositoryBase
{
    private static readonly Regex TableChangeRegex = new(
        WriteSetConsts.TableChangePattern,
        RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex ColumnChangeRegex = new(
        WriteSetConsts.ColumnChangePattern,
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    // islevi: Provider destekliyorsa normal PostgreSQL capability sorgulari icin guvenli profilli baglanti kurar.
    protected virtual NpgsqlConnection? CreateWriteSetProbeConnection(DatabaseConnectionInfo info)
        => null;

    // islevi: Provider destekliyorsa Npgsql logical replication protokolu icin guvenli profilli baglanti kurar.
    protected virtual LogicalReplicationConnection? CreateWriteSetReplicationConnection(DatabaseConnectionInfo info)
        => null;

    // islevi: wal_level ve current role replication bayraklarini ayni provider baglantisinda exception siz olcer.
    public async Task<CapabilityLevel> ProbeAsync(
        DatabaseConnectionInfo info,
        CancellationToken cancellationToken = default)
    {
        await using var connection = CreateWriteSetProbeConnection(info);
        if (connection is null)
        {
            return UnsupportedCapability();
        }

        try
        {
            await connection.OpenAsync(cancellationToken);
            var walLevel = await ReadScalarAsync<string>(
                connection, WriteSetConsts.ShowWalLevelSql, cancellationToken);
            var canReplicate = await ReadScalarAsync<bool>(
                connection, WriteSetConsts.CanReplicateSql, cancellationToken);
            return CreateProbeFacts(walLevel, canReplicate);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (NpgsqlException)
        {
            return UnavailableCapability(CapabilityReasonCodes.NoCapability);
        }
    }

    // islevi: Aday tablolarin count baslangicini alir, temporary test_decoding slotunu dinler ve ayni oturumda garantili dusurur.
    public async Task<WriteSetResult> CaptureAsync(
        DatabaseConnectionInfo info,
        Guid captureRef,
        List<ComparisonTableIdentifierModel> candidateTables,
        CorrelationRef? correlation,
        CancellationToken cancellationToken = default)
    {
        await using var connection = CreateWriteSetReplicationConnection(info);
        if (connection is null)
        {
            return UnsupportedResult();
        }

        var beforeCounts = await ReadRowCountsAsync(info, candidateTables, cancellationToken);
        var result = ExactResult();
        var slotName = BuildSlotName(captureRef);
        var slotCreated = false;
        try
        {
            await connection.Open(cancellationToken);
            var slot = await connection.CreateTestDecodingReplicationSlot(
                slotName,
                temporarySlot: true,
                slotSnapshotInitMode: LogicalSlotSnapshotInitMode.NoExport,
                twoPhase: false,
                cancellationToken);
            slotCreated = true;
            var messages = await CaptureMessagesAsync(connection, slot, cancellationToken);
            var afterCounts = await ReadRowCountsAsync(info, candidateTables, cancellationToken);
            result = BuildExactResult(candidateTables, beforeCounts, afterCounts, messages);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (NpgsqlException)
        {
            result = UnavailableResult(CapabilityReasonCodes.NoCapability);
        }
        finally
        {
            if (slotCreated && !await TryDropSlotAsync(connection, slotName))
            {
                result.Reasons.Add(CapabilityReasonCodes.SlotReleaseFailed);
            }
        }

        return result;
    }

    // islevi: CaptureRef slotunu yeni replication oturumunda idempotent dusurur; temporary slot zaten yoksa basarili sayar.
    public async Task<bool> ReleaseAsync(
        DatabaseConnectionInfo info,
        Guid captureRef,
        CancellationToken cancellationToken = default)
    {
        await using var connection = CreateWriteSetReplicationConnection(info);
        if (connection is null)
        {
            return true;
        }

        try
        {
            await connection.Open(cancellationToken);
            return await TryDropSlotAsync(connection, BuildSlotName(captureRef));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (NpgsqlException)
        {
            return false;
        }
    }

    // islevi: Capture penceresi boyunca test_decoding mesajlarini kopyalar ve her WAL konumunu tuketilmis olarak bildirir.
    private static async Task<List<string>> CaptureMessagesAsync(
        LogicalReplicationConnection connection,
        TestDecodingReplicationSlot slot,
        CancellationToken cancellationToken)
    {
        using var captureSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        captureSource.CancelAfter(WriteSetConsts.CaptureTimeoutMilliseconds);
        var messages = new List<string>();
        try
        {
            await foreach (var message in connection.StartReplication(slot, captureSource.Token))
            {
                messages.Add(message.Data);
                connection.SetReplicationStatus(message.WalEnd);
            }
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
        }

        return messages;
    }

    // islevi: Slot drop'u cagiran cancellation'ina baglamadan sinirli surede dener ve yok slotu idempotent basari sayar.
    private static async Task<bool> TryDropSlotAsync(
        LogicalReplicationConnection connection,
        string slotName)
    {
        using var releaseSource = new CancellationTokenSource(
            WriteSetConsts.SlotReleaseTimeoutMilliseconds);
        try
        {
            await connection.DropReplicationSlot(
                slotName, wait: false, releaseSource.Token);
            return true;
        }
        catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.UndefinedObject)
        {
            return true;
        }
        catch (NpgsqlException)
        {
            return false;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }

    // islevi: Decoded table/column adresleri ile once/sonra count'lari exact advisory sonuca indirger.
    internal static WriteSetResult BuildExactResult(
        List<ComparisonTableIdentifierModel> candidateTables,
        List<TableRowCountModel> beforeCounts,
        List<TableRowCountModel> afterCounts,
        IEnumerable<string> messages)
    {
        var candidates = candidateTables.Select(FormatTable)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var tables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var message in messages)
        {
            AddDecodedChange(message, candidates, tables, columns);
        }

        return new WriteSetResult
        {
            StrengthCode = FootprintStrengthCodes.Exact,
            Tables = tables.Order(StringComparer.OrdinalIgnoreCase).ToList(),
            Columns = columns.Order(StringComparer.OrdinalIgnoreCase).ToList(),
            RowDeltas = BuildRowDeltas(tables, beforeCounts, afterCounts),
            IsAdvisoryOnly = true
        };
    }

    // islevi: Tek test_decoding mesajini candidate table ve schema.table.column referanslarina cevirir.
    private static void AddDecodedChange(
        string message,
        IReadOnlySet<string> candidates,
        ISet<string> tables,
        ISet<string> columns)
    {
        var match = TableChangeRegex.Match(message);
        if (!match.Success)
        {
            return;
        }

        var table = FormatTable(match.Groups["schema"].Value, match.Groups["table"].Value);
        if (!candidates.Contains(table))
        {
            return;
        }

        tables.Add(table);
        foreach (Match column in ColumnChangeRegex.Matches(match.Groups["payload"].Value))
        {
            columns.Add($"{table}.{column.Groups["column"].Value}");
        }
    }

    // islevi: Degisen tablolar icin once/sonra kesin count ve signed delta item'larini kararli sirada kurar.
    private static List<WriteSetTableDelta> BuildRowDeltas(
        IEnumerable<string> tables,
        IEnumerable<TableRowCountModel> beforeCounts,
        IEnumerable<TableRowCountModel> afterCounts)
    {
        var before = BuildRowCountMap(beforeCounts);
        var after = BuildRowCountMap(afterCounts);
        return tables.Order(StringComparer.OrdinalIgnoreCase)
            .Select(table => CreateRowDelta(table, before, after))
            .ToList();
    }

    // islevi: Row count listesini schema.table anahtarina gore case-insensitive sozluge cevirir.
    private static Dictionary<string, long> BuildRowCountMap(IEnumerable<TableRowCountModel> rowCounts)
        => rowCounts.ToDictionary(
            item => FormatTable(item.SchemaName, item.TableName),
            item => item.RowCount,
            StringComparer.OrdinalIgnoreCase);

    // islevi: Tek tablo icin eksik count'u sifir kabul edip after-before signed delta uretir.
    private static WriteSetTableDelta CreateRowDelta(
        string table,
        IReadOnlyDictionary<string, long> before,
        IReadOnlyDictionary<string, long> after)
    {
        var beforeCount = before.GetValueOrDefault(table);
        var afterCount = after.GetValueOrDefault(table);
        return new WriteSetTableDelta
        {
            Table = table,
            BeforeRowCount = beforeCount,
            AfterRowCount = afterCount,
            Delta = afterCount - beforeCount
        };
    }

    // islevi: Tek scalar PostgreSQL capability sorgusunu tipli CLR degerine cevirir.
    private static async Task<T> ReadScalarAsync<T>(
        NpgsqlConnection connection,
        string commandText,
        CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand(commandText, connection);
        var value = await command.ExecuteScalarAsync(cancellationToken);
        return (T)Convert.ChangeType(value!, typeof(T), CultureInfo.InvariantCulture);
    }

    // islevi: wal_level ve role olgularini kapali probe gerekcelerine cevirir.
    private static CapabilityLevel CreateProbeFacts(string walLevel, bool canReplicate)
    {
        var hasLogicalWal = string.Equals(
            walLevel, WriteSetConsts.LogicalWalLevel, StringComparison.OrdinalIgnoreCase);
        return new CapabilityLevel
        {
            HasLogicalDecoding = hasLogicalWal && canReplicate,
            Reasons = hasLogicalWal
                ? canReplicate ? [] : [CapabilityReasonCodes.NoReplicationGrant]
                : [CapabilityReasonCodes.WalLevelNotLogical]
        };
    }

    // islevi: CaptureRef'i PostgreSQL slot ad limitine uygun kararli compact isme cevirir.
    private static string BuildSlotName(Guid captureRef)
        => WriteSetConsts.SlotNamePrefix +
           captureRef.ToString(WriteSetConsts.CompactCaptureRefFormat, CultureInfo.InvariantCulture);

    // islevi: Provider-notr tablo kimligini schema.table sonuc adresine cevirir.
    private static string FormatTable(ComparisonTableIdentifierModel table)
        => FormatTable(table.SchemaName, table.TableName);

    // islevi: Sema ve tabloyu schema.table sonuc adresine cevirir.
    private static string FormatTable(string schemaName, string tableName)
        => $"{schemaName}.{tableName}";

    // islevi: Desteklenmeyen provider icin exception yerine kapali capability sonucu kurar.
    private static CapabilityLevel UnsupportedCapability()
        => UnavailableCapability(CapabilityReasonCodes.EngineNotSupported);

    // islevi: Tek gerekceli unavailable capability olgusu kurar.
    private static CapabilityLevel UnavailableCapability(string reason)
        => new()
        {
            StrengthCode = FootprintStrengthCodes.Unavailable,
            Reasons = [reason]
        };

    // islevi: Desteklenmeyen provider icin advisory capture sonucu kurar.
    private static WriteSetResult UnsupportedResult()
        => UnavailableResult(CapabilityReasonCodes.EngineNotSupported);

    // islevi: Bos exact advisory capture sonucu kurar.
    private static WriteSetResult ExactResult()
        => new() { StrengthCode = FootprintStrengthCodes.Exact, IsAdvisoryOnly = true };

    // islevi: Tek gerekceli unavailable advisory capture sonucu kurar.
    private static WriteSetResult UnavailableResult(string reason)
        => new()
        {
            StrengthCode = FootprintStrengthCodes.Unavailable,
            IsAdvisoryOnly = true,
            Reasons = [reason]
        };
}
