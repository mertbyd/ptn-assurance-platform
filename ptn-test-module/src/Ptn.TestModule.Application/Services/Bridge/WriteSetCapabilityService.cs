using System;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.ExceptionCodes;
using Ptn.DatabaseChecker.Interface.Connections;
using Ptn.DatabaseChecker.Managers.Connections;
using Ptn.DatabaseChecker.Models.Comparison;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Dtos.Bridge;
using Ptn.TestModule.ExceptionCodes.Bridge;
using Ptn.TestModule.Managers.Bridge;
using Ptn.TestModule.Mappers.Bridge;
using Ptn.TestModule.Models.Bridge.Footprint;
using Volo.Abp;

namespace Ptn.TestModule.Services.Bridge;

// islevi: Kayitli PostgreSQL baglantisinda WAL ve replication yetenegini yoklar, temporary slotu yonetir.
// sistemdeki gorevi: Npgsql I/O ve provider hata cevirisini Application sinirinda tutup domain stratejisine olgu verir.
[RemoteService(IsEnabled = false)]
public class WriteSetCapabilityService : TestModuleAppService, IWriteSetCapabilityService
{
    private static readonly PtnBridgeMapper Mapper = new();
    private readonly IDatabaseConnectionRepository _connectionRepository;
    private readonly DatabaseConnectionInfoFactory _connectionInfoFactory;
    private readonly FootprintCapabilityManager _manager;

    public WriteSetCapabilityService(
        IDatabaseConnectionRepository connectionRepository,
        DatabaseConnectionInfoFactory connectionInfoFactory,
        FootprintCapabilityManager manager)
    {
        _connectionRepository = connectionRepository;
        _connectionInfoFactory = connectionInfoFactory;
        _manager = manager;
    }

    // Public capability istegini domain sonucundan DTO'ya map eder.
    public async Task<PtnCapabilityLevelDto> ProbeCapabilityAsync(
        Guid connectionId,
        bool hasExclusiveSandbox,
        CancellationToken cancellationToken)
    {
        var info = await GetPostgreSqlInfoAsync(connectionId, cancellationToken);
        if (info is null || !hasExclusiveSandbox)
        {
            return Mapper.Map(FootprintCapabilityManager.CreateUnavailableCapability(hasExclusiveSandbox));
        }

        try
        {
            await using var connection = await OpenAsync(info, cancellationToken);
            var walLevel = await ScalarAsync<string>(connection, PtnWriteSetSql.ShowWalLevel, cancellationToken);
            var canReplicate = await ScalarAsync<bool>(connection, PtnWriteSetSql.CanReplicate, cancellationToken);
            return Mapper.Map(_manager.ResolveCapability(
                walLevel == PtnWriteSetSql.LogicalWalLevel,
                canReplicate,
                true));
        }
        catch (NpgsqlException)
        {
            return Mapper.Map(FootprintCapabilityManager.CreateUnavailableCapability(true));
        }
    }

    // Public capture istegini advisory footprint DTO sonucuna map eder.
    public async Task<PtnFootprintResultDto> CaptureWriteSetAsync(
        Guid connectionId,
        Guid captureId,
        CancellationToken cancellationToken)
    {
        var info = await GetPostgreSqlInfoAsync(connectionId, cancellationToken);
        if (info is null)
        {
            return Mapper.Map(FootprintCapabilityManager.CreateUnavailableFootprint(
                [TestModuleBridgeErrorCodes.EvidenceUnavailable]));
        }

        await using var connection = await OpenAsync(info, cancellationToken);
        var slotName = SlotName(captureId);
        try
        {
            await ExecuteAsync(connection, PtnWriteSetSql.CreateTemporarySlot, slotName, cancellationToken);
            await ExecuteAsync(connection, PtnWriteSetSql.ReadChanges, slotName, cancellationToken);
            return Mapper.Map(FootprintCapabilityManager.EnsureAdvisory(
                FootprintCapabilityManager.CreateUnavailableFootprint(
                    [TestModuleBridgeErrorCodes.EvidenceUnavailable])));
        }
        finally
        {
            await ExecuteAsync(connection, PtnWriteSetSql.DropSlot, slotName, cancellationToken);
        }
    }

    // Kalabilen slotu yeni baglantida idempotent olarak dusurur.
    public async Task ReleaseAsync(
        Guid connectionId,
        Guid captureId,
        CancellationToken cancellationToken)
    {
        var info = await GetPostgreSqlInfoAsync(connectionId, cancellationToken);
        if (info is null)
        {
            return;
        }

        await using var connection = await OpenAsync(info, cancellationToken);
        await ExecuteAsync(connection, PtnWriteSetSql.DropSlot, SlotName(captureId), cancellationToken);
    }

    // Checker adres defteri ve Vault kimligini tek PostgreSQL runtime baglantisina cozer.
    private async Task<DatabaseConnectionInfo?> GetPostgreSqlInfoAsync(
        Guid connectionId,
        CancellationToken cancellationToken)
    {
        var connection = await _connectionRepository.GetWithDetailsAsync(connectionId, cancellationToken);
        return connection.Engine.Code == DatabaseEngineCodes.PostgreSql
            ? await _connectionInfoFactory.BuildAsync(connection, cancellationToken)
            : null;
    }

    // Runtime baglanti modelini Npgsql baglantisina cevirip acar.
    private static async Task<NpgsqlConnection> OpenAsync(
        DatabaseConnectionInfo info,
        CancellationToken cancellationToken)
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = info.Host,
            Port = info.Port,
            Database = info.DatabaseName,
            Username = info.Username,
            Password = info.Password,
            Timeout = info.SafetyProfile.ConnectTimeoutSeconds,
            CommandTimeout = info.SafetyProfile.StatementTimeoutSeconds,
            ApplicationName = info.SafetyProfile.ApplicationName,
            SslMode = ResolvePostgreSqlSslMode(info.SafetyProfile.TlsModeCode)
        };
        var connection = new NpgsqlConnection(builder.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    // Checker safety profile'indaki kararli TLS kodunu Npgsql moduna cevirir.
    private static SslMode ResolvePostgreSqlSslMode(string tlsModeCode) =>
        tlsModeCode switch
        {
            TlsModeCodes.Require => SslMode.Require,
            TlsModeCodes.Prefer => SslMode.Prefer,
            TlsModeCodes.Disable => SslMode.Disable,
            _ => throw new BusinessException(DatabaseConnectionExceptionCodes.InvalidTlsMode)
        };

    // Tek scalar capability sorgusunu tipli sonuc olarak okur.
    private static async Task<T> ScalarAsync<T>(
        NpgsqlConnection connection,
        string commandText,
        CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand(commandText, connection);
        var value = await command.ExecuteScalarAsync(cancellationToken);
        return (T)Convert.ChangeType(value!, typeof(T));
    }

    // Slot sorgusunu yalniz parametreli slot kimligiyle calistirir.
    private static async Task ExecuteAsync(
        NpgsqlConnection connection,
        string commandText,
        string slotName,
        CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand(commandText, connection);
        command.Parameters.AddWithValue(PtnWriteSetSql.SlotNameParameter, NpgsqlDbType.Text, slotName);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    // Capture kimligini guvenli PostgreSQL identifier uzunlugunda slot adina cevirir.
    private static string SlotName(Guid captureId) =>
        PtnBridgeConsts.WriteSetSlotPrefix + captureId.ToString(PtnBridgeConsts.CompactIdFormat);
}
