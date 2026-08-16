using Ptn.DatabaseChecker.Constants.Capabilities;
using Ptn.DatabaseChecker.Entities.Connections;
using Ptn.DatabaseChecker.ExceptionCodes;
using Ptn.DatabaseChecker.Interface.Capabilities;
using Ptn.DatabaseChecker.Interface.Comparison;
using Ptn.DatabaseChecker.Managers.Connections;
using Ptn.DatabaseChecker.Models.Capabilities;
using Ptn.DatabaseChecker.Models.Comparison;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace Ptn.DatabaseChecker.Managers.Capabilities;

// islevi: Sandbox, motor, logical decoding ve diff fallback olgularini baglayici sirayla yazma kumesi seviyesine cevirir.
// sistemdeki gorevi: AppService'i strateji, advisory degismezi, audit gurultusu ve garantili slot release kararlarindan uzak tutar.
public class WriteSetCapabilityManager : DomainService
{
    private readonly DatabaseConnectionInfoFactory _connectionInfoFactory;
    private readonly IEngineComponentResolver<IWriteSetRepository> _writeSetRepositoryResolver;
    private readonly IEngineComponentResolver<IDatabaseDataComparisonRepository> _dataRepositoryResolver;
    private readonly IDiffWriteSetRepository _diffRepository;

    // islevi: Capability stratejisini mevcut secret, engine resolver ve diff sahipleriyle kurar.
    public WriteSetCapabilityManager(
        DatabaseConnectionInfoFactory connectionInfoFactory,
        IEngineComponentResolver<IWriteSetRepository> writeSetRepositoryResolver,
        IEngineComponentResolver<IDatabaseDataComparisonRepository> dataRepositoryResolver,
        IDiffWriteSetRepository diffRepository)
    {
        _connectionInfoFactory = connectionInfoFactory;
        _writeSetRepositoryResolver = writeSetRepositoryResolver;
        _dataRepositoryResolver = dataRepositoryResolver;
        _diffRepository = diffRepository;
    }

    // islevi: Sandbox -> motor -> logical decoding -> diff -> unavailable siralamasini exception siz sonuc sozlesmesine uygular.
    public virtual async Task<CapabilityLevel> ProbeAsync(
        DatabaseConnection? connection,
        CapabilityProbeRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!request.RequiresExclusiveSandbox)
        {
            return CreateUnavailable(false, CapabilityReasonCodes.SharedEnvironment);
        }

        var repository = ResolveWriteSetRepository(connection);
        if (repository is null)
        {
            return CreateUnavailable(true, ResolveMissingConnectionReason(connection));
        }

        var facts = await ReadProviderFactsAsync(connection!, repository, cancellationToken);
        return ResolveCapability(connection!, facts);
    }

    // islevi: Olculen seviyeye gore exact temporary-slot veya mevcut-motor diff capture yolunu calistirir.
    public virtual async Task<WriteSetResult> CaptureAsync(
        DatabaseConnection? connection,
        WriteSetCaptureRequest request,
        CancellationToken cancellationToken = default)
    {
        var repository = ResolveWriteSetRepository(connection);
        if (repository is null)
        {
            return CreateUnavailableResult(ResolveMissingConnectionReason(connection));
        }

        var info = await BuildInfoOrNullAsync(connection!, cancellationToken);
        var capability = info is null
            ? CreateUnavailable(true, CapabilityReasonCodes.NoCapability)
            : ResolveCapability(connection!, await SafeProbeAsync(repository, info, cancellationToken));
        return await CaptureByCapabilityAsync(
            connection!, info, repository, request, capability, cancellationToken);
    }

    // islevi: CaptureRef ile iliskili kalmis slotu idempotent temizler ve cleanup basarisizligini sonuc gerekcesine tasir.
    public virtual async Task<WriteSetResult> ReleaseAsync(
        DatabaseConnection? connection,
        Guid captureRef,
        CancellationToken cancellationToken = default)
    {
        var repository = ResolveWriteSetRepository(connection);
        if (repository is null)
        {
            return CreateUnavailableResult(ResolveMissingConnectionReason(connection));
        }

        var info = await BuildInfoOrNullAsync(connection!, cancellationToken);
        if (info is null)
        {
            return CreateUnavailableResult(CapabilityReasonCodes.NoCapability);
        }

        return await ReleaseResourceAsync(
            connection!, repository, info, captureRef, cancellationToken);
    }

    // islevi: Exact veya inferred seviyeyi ilgili repository yoluna yonlendirir; unavailable seviyeyi bos advisory sonuc yapar.
    private async Task<WriteSetResult> CaptureByCapabilityAsync(
        DatabaseConnection connection,
        DatabaseConnectionInfo? info,
        IWriteSetRepository repository,
        WriteSetCaptureRequest request,
        CapabilityLevel capability,
        CancellationToken cancellationToken)
    {
        if (info is null || capability.StrengthCode == FootprintStrengthCodes.Unavailable)
        {
            return CreateResult(capability);
        }

        var tables = CreateCandidateTables(request.CandidateTables);
        return capability.StrengthCode == FootprintStrengthCodes.Exact
            ? await CaptureExactAsync(repository, info, request, tables, capability, cancellationToken)
            : await CaptureInferredAsync(connection, request, tables, capability, cancellationToken);
    }

    // islevi: Logical capture'i calistirir ve basari/hata/cancellation yolunun tamaminda idempotent release dener.
    private async Task<WriteSetResult> CaptureExactAsync(
        IWriteSetRepository repository,
        DatabaseConnectionInfo info,
        WriteSetCaptureRequest request,
        List<ComparisonTableIdentifierModel> tables,
        CapabilityLevel capability,
        CancellationToken cancellationToken)
    {
        WriteSetResult result;
        var released = false;
        try
        {
            result = await repository.CaptureAsync(
                info, request.CaptureRef, tables, request.Correlation, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            result = CreateUnavailableResult(CapabilityReasonCodes.NoCapability);
        }
        finally
        {
            released = await TryReleaseAsync(repository, info, request.CaptureRef);
        }

        if (!released)
        {
            result.Reasons.Add(CapabilityReasonCodes.SlotReleaseFailed);
        }

        return FinalizeResult(ApplyCapability(result, capability));
    }

    // islevi: Logical decoding yokken ayni capture penceresini mevcut row-count ve table-data motorlarina delege eder.
    private async Task<WriteSetResult> CaptureInferredAsync(
        DatabaseConnection connection,
        WriteSetCaptureRequest request,
        List<ComparisonTableIdentifierModel> tables,
        CapabilityLevel capability,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _diffRepository.CaptureAsync(
                connection, tables, request.Correlation, cancellationToken);
            return FinalizeResult(ApplyCapability(result, capability));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            return CreateUnavailableResult(CapabilityReasonCodes.NoCapability);
        }
    }

    // islevi: Release sonucunu guncel capability seviyesiyle kurar ve cleanup basarisizligini kapali gerekceye cevirir.
    private async Task<WriteSetResult> ReleaseResourceAsync(
        DatabaseConnection connection,
        IWriteSetRepository repository,
        DatabaseConnectionInfo info,
        Guid captureRef,
        CancellationToken cancellationToken)
    {
        var capability = ResolveCapability(
            connection, await SafeProbeAsync(repository, info, cancellationToken));
        var result = CreateResult(capability);
        if (!await TryReleaseAsync(repository, info, captureRef))
        {
            result.Reasons.Add(CapabilityReasonCodes.SlotReleaseFailed);
        }

        return FinalizeResult(result);
    }

    // islevi: Provider probe hatalarini capability sozlesmesindeki NoCapability sonucuna cevirir.
    private async Task<CapabilityLevel> ReadProviderFactsAsync(
        DatabaseConnection connection,
        IWriteSetRepository repository,
        CancellationToken cancellationToken)
    {
        var info = await BuildInfoOrNullAsync(connection, cancellationToken);
        return info is null
            ? CreateUnavailable(true, CapabilityReasonCodes.NoCapability)
            : await SafeProbeAsync(repository, info, cancellationToken);
    }

    // islevi: Repository probe'unun dis baglanti hatalarini kapali sonuc yapar; gercek HTTP cancellation'i korur.
    private static async Task<CapabilityLevel> SafeProbeAsync(
        IWriteSetRepository repository,
        DatabaseConnectionInfo info,
        CancellationToken cancellationToken)
    {
        try
        {
            return await repository.ProbeAsync(info, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            return CreateUnavailable(true, CapabilityReasonCodes.NoCapability);
        }
    }

    // islevi: Olculen provider olgularini baglayici probe sirasi ve comparison fallback varligiyla guc seviyesine cevirir.
    private CapabilityLevel ResolveCapability(DatabaseConnection connection, CapabilityLevel facts)
    {
        facts.HasExclusiveSandbox = true;
        if (facts.Reasons.Contains(CapabilityReasonCodes.EngineNotSupported) ||
            facts.Reasons.Contains(CapabilityReasonCodes.NoCapability))
        {
            facts.StrengthCode = FootprintStrengthCodes.Unavailable;
            return facts;
        }

        if (facts.HasLogicalDecoding)
        {
            facts.StrengthCode = FootprintStrengthCodes.Exact;
            facts.Reasons.Clear();
            return facts;
        }

        facts.StrengthCode = HasComparisonRepository(connection.Engine.Code)
            ? FootprintStrengthCodes.Inferred
            : FootprintStrengthCodes.Unavailable;
        if (facts.StrengthCode == FootprintStrengthCodes.Unavailable)
        {
            facts.Reasons = [CapabilityReasonCodes.NoCapability];
        }

        return facts;
    }

    // islevi: Motor kodunun mevcut data comparison repository ailesinde cozulup cozulmedigini bildirir.
    private bool HasComparisonRepository(string engineCode)
    {
        try
        {
            _dataRepositoryResolver.Resolve(engineCode);
            return true;
        }
        catch (BusinessException exception) when (exception.Code == ComparisonExceptionCodes.UnsupportedEngine)
        {
            return false;
        }
    }

    // islevi: Motor bilesenini resolver ile secer; eksik baglanti veya desteklenmeyen motoru null olguya cevirir.
    private IWriteSetRepository? ResolveWriteSetRepository(DatabaseConnection? connection)
    {
        if (connection?.Engine is null)
        {
            return null;
        }

        try
        {
            return _writeSetRepositoryResolver.Resolve(connection.Engine.Code);
        }
        catch (BusinessException exception) when (exception.Code == ComparisonExceptionCodes.UnsupportedEngine)
        {
            return null;
        }
    }

    // islevi: Secret ve safety profile cozumleme hatasini public capability seviyesinde NoCapability yapar.
    private async Task<DatabaseConnectionInfo?> BuildInfoOrNullAsync(
        DatabaseConnection connection,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _connectionInfoFactory.BuildAsync(connection, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            return null;
        }
    }

    // islevi: Cleanup'i cagiran cancellation'ina baglamadan dener ve her provider hatasini false olguya cevirir.
    private static async Task<bool> TryReleaseAsync(
        IWriteSetRepository repository,
        DatabaseConnectionInfo info,
        Guid captureRef)
    {
        try
        {
            return await repository.ReleaseAsync(info, captureRef, CancellationToken.None);
        }
        catch (Exception)
        {
            return false;
        }
    }

    // islevi: Katalog-onayli schema.table adaylarini provider-notr tablo kimliklerine cevirir.
    private static List<ComparisonTableIdentifierModel> CreateCandidateTables(IEnumerable<string> candidates)
        => candidates.Select(candidate => candidate.Split('.', 2, StringSplitOptions.TrimEntries))
            .Select(parts => new ComparisonTableIdentifierModel
            {
                SchemaName = parts[0],
                TableName = parts[1]
            })
            .DistinctBy(table => FormatTable(table.SchemaName, table.TableName), StringComparer.OrdinalIgnoreCase)
            .ToList();

    // islevi: Capability olgularini capture sonucuna uygular ve provider gerekcelerini tekrar etmeden birlestirir.
    private static WriteSetResult ApplyCapability(WriteSetResult result, CapabilityLevel capability)
    {
        if (string.IsNullOrWhiteSpace(result.StrengthCode))
        {
            result.StrengthCode = capability.StrengthCode;
        }

        result.Reasons.AddRange(capability.Reasons);
        return result;
    }

    // islevi: Inferred audit-only degisimleri eler, Exact kolonlari temizler ve advisory bayragini zorunlu true yapar.
    private static WriteSetResult FinalizeResult(WriteSetResult result)
    {
        var auditColumns = WriteSetConsts.AuditColumnNames.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var visibleColumns = result.Columns
            .Where(column => !auditColumns.Contains(ResolveColumnName(column)))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (result.StrengthCode == FootprintStrengthCodes.Inferred)
        {
            RemoveInferredAuditOnlyTables(result, visibleColumns);
            visibleColumns.Clear();
        }

        result.Columns = visibleColumns;
        result.Tables = result.Tables.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        result.Reasons = result.Reasons.Distinct(StringComparer.Ordinal).ToList();
        result.IsAdvisoryOnly = true;
        return result;
    }

    // islevi: Sadece audit kolonlari degisen ve satir sayisi ayni kalan inferred tablolari advisory footprint'ten cikarir.
    private static void RemoveInferredAuditOnlyTables(WriteSetResult result, IEnumerable<string> visibleColumns)
    {
        var changedTables = visibleColumns.Select(ResolveColumnTable)
            .Concat(result.RowDeltas.Where(delta => delta.Delta != 0).Select(delta => delta.Table))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        result.Tables = result.Tables.Where(changedTables.Contains).ToList();
        result.RowDeltas = result.RowDeltas.Where(delta => changedTables.Contains(delta.Table)).ToList();
    }

    // islevi: schema.table.column referansindan kolon adini alir.
    private static string ResolveColumnName(string columnRef)
        => columnRef[(columnRef.LastIndexOf('.') + 1)..];

    // islevi: schema.table.column referansindan tablo adresini alir.
    private static string ResolveColumnTable(string columnRef)
    {
        var separator = columnRef.LastIndexOf('.');
        return separator < 0 ? string.Empty : columnRef[..separator];
    }

    // islevi: Sema ve tabloyu candidate/public sonuc icin kararli basit adrese cevirir.
    private static string FormatTable(string schemaName, string tableName)
        => $"{schemaName}.{tableName}";

    // islevi: Baglanti yoklugu ile resolver'da bulunmayan motoru kapali gerekceye cevirir.
    private static string ResolveMissingConnectionReason(DatabaseConnection? connection)
        => connection?.Engine is null
            ? CapabilityReasonCodes.NoCapability
            : CapabilityReasonCodes.EngineNotSupported;

    // islevi: Tek gerekceli unavailable capability sonucu kurar.
    private static CapabilityLevel CreateUnavailable(bool exclusive, string reason)
        => new()
        {
            StrengthCode = FootprintStrengthCodes.Unavailable,
            HasExclusiveSandbox = exclusive,
            Reasons = [reason]
        };

    // islevi: Capability seviyesini ayni advisory capture sonuc sekline cevirir.
    private static WriteSetResult CreateResult(CapabilityLevel capability)
        => FinalizeResult(new WriteSetResult
        {
            StrengthCode = capability.StrengthCode,
            Reasons = capability.Reasons.ToList()
        });

    // islevi: Tek gerekceli unavailable advisory capture sonucu kurar.
    private static WriteSetResult CreateUnavailableResult(string reason)
        => FinalizeResult(new WriteSetResult
        {
            StrengthCode = FootprintStrengthCodes.Unavailable,
            Reasons = [reason]
        });
}
