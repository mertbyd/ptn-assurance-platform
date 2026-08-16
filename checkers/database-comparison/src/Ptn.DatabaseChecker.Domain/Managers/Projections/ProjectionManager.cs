using Ptn.DatabaseChecker.Constants.Comparison.Projections;
using Ptn.DatabaseChecker.Entities.Connections;
using Ptn.DatabaseChecker.ExceptionCodes;
using Ptn.DatabaseChecker.Interface.Comparison;
using Ptn.DatabaseChecker.Interface.Projections;
using Ptn.DatabaseChecker.Managers.Comparison;
using Ptn.DatabaseChecker.Managers.Connections;
using Ptn.DatabaseChecker.Models.Comparison;
using Ptn.DatabaseChecker.Models.Projections;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace Ptn.DatabaseChecker.Managers.Projections;

// islevi: Projection hedefini katalogda dogrular, butceyi uygular, satirlari okur ve retention politikasiyla redakte eder.
// sistemdeki gorevi: AppService'i katalog, unique-key, truncation, authorization outcome ve gizlilik kararlarindan uzak tutar.
public class ProjectionManager : DomainService
{
    private readonly DatabaseDataComparisonManager _dataManager;
    private readonly DatabaseConnectionInfoFactory _connectionInfoFactory;
    private readonly IEngineComponentResolver<IProjectionRepository> _repositoryResolver;
    private readonly ValueRetentionPolicyResolver _retentionPolicyResolver;
    private readonly FindingValueRedactor _redactor;

    // islevi: Projection cekirdegini mevcut katalog, secret, resolver ve redaction sahipleriyle kurar.
    public ProjectionManager(
        DatabaseDataComparisonManager dataManager,
        DatabaseConnectionInfoFactory connectionInfoFactory,
        IEngineComponentResolver<IProjectionRepository> repositoryResolver,
        ValueRetentionPolicyResolver retentionPolicyResolver,
        FindingValueRedactor redactor)
    {
        _dataManager = dataManager;
        _connectionInfoFactory = connectionInfoFactory;
        _repositoryResolver = repositoryResolver;
        _retentionPolicyResolver = retentionPolicyResolver;
        _redactor = redactor;
    }

    // islevi: Katalog guvenligi, satir butcesi, provider okuma ve redaction adimlarini sirayla calistirir.
    public virtual async Task<ProjectionResult> ProjectAsync(
        DatabaseConnection connection,
        ProjectionRequest request,
        CancellationToken cancellationToken = default)
    {
        var structure = await _dataManager.ResolveAssertionStructureAsync(
            connection, request.SchemaName, request.TableName, cancellationToken);
        var catalogOutcome = ResolveCatalogOutcome(structure, request);
        if (catalogOutcome is not null)
        {
            return CreateResult(catalogOutcome, request);
        }

        var maxRows = ResolveMaxRows(request.MaxRows);
        var rows = await ReadRowsAsync(connection, structure!, request, maxRows + 1, cancellationToken);
        if (rows is null)
        {
            return CreateResult(ProjectionOutcomeCodes.NotAuthorized, request);
        }

        return await BuildProjectedResultAsync(rows, request, maxRows);
    }

    // islevi: Tablo, kolon ve unique anahtar kapilarinin ilk basarisiz outcome'unu cozer.
    private static string? ResolveCatalogOutcome(TableDataStructureModel? structure, ProjectionRequest request)
    {
        if (structure is null)
        {
            return ProjectionOutcomeCodes.TableNotFound;
        }

        var columns = structure.ColumnNames.ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (request.KeyValues.Keys.Any(column => !columns.Contains(column)) ||
            request.ProjectColumns.Any(column => !columns.Contains(column)))
        {
            return ProjectionOutcomeCodes.ColumnNotFound;
        }

        return IsUniqueKey(structure, request.KeyValues.Keys)
            ? null
            : ProjectionOutcomeCodes.KeyNotUnique;
    }

    // islevi: Istek anahtar kumesinin katalogdaki PK veya filtresiz unique anahtarla tam eslestigini bildirir.
    private static bool IsUniqueKey(TableDataStructureModel structure, IEnumerable<string> keyColumns)
    {
        var requested = keyColumns.ToHashSet(StringComparer.OrdinalIgnoreCase);
        return structure.UniqueKeyColumnSets.Any(key =>
            key.Count == requested.Count && key.All(requested.Contains));
    }

    // islevi: Opsiyonel satir butcesini urun varsayilani ve sert tavan arasinda cozer.
    private static int ResolveMaxRows(int? requestedMaxRows)
        => Math.Clamp(
            requestedMaxRows ?? ProjectionConsts.DefaultMaxRows,
            1,
            ProjectionConsts.MaxRowsCeiling);

    // islevi: Motor bilesenini cozer ve yalniz yetki reddini kapali projection outcome'una cevirir.
    private async Task<List<ProjectionRow>?> ReadRowsAsync(
        DatabaseConnection connection,
        TableDataStructureModel structure,
        ProjectionRequest request,
        int readLimit,
        CancellationToken cancellationToken)
    {
        try
        {
            var info = await _connectionInfoFactory.BuildAsync(connection, cancellationToken);
            var repository = _repositoryResolver.Resolve(connection.Engine.Code);
            return await repository.ReadProjectionRowsAsync(
                info, structure, request.KeyValues, request.ProjectColumns, readLimit, cancellationToken);
        }
        catch (BusinessException exception) when (exception.Code == AssertionExceptionCodes.ProjectionNotAuthorized)
        {
            return null;
        }
    }

    // islevi: Fazladan okunan sentinel satirla truncation'i aciklar ve gorunen satirlari mevcut retention politikasiyla redakte eder.
    private async Task<ProjectionResult> BuildProjectedResultAsync(
        List<ProjectionRow> rows,
        ProjectionRequest request,
        int maxRows)
    {
        var truncated = rows.Count > maxRows;
        var policy = await _retentionPolicyResolver.ResolveAsync();
        var visibleRows = rows.Take(maxRows)
            .Select(row => new ProjectionRow(row.ToDictionary(
                pair => pair.Key,
                pair => _redactor.Redact(pair.Value, policy),
                StringComparer.OrdinalIgnoreCase)))
            .ToList();
        return new ProjectionResult
        {
            OutcomeCode = truncated ? ProjectionOutcomeCodes.Truncated : ProjectionOutcomeCodes.Projected,
            Rows = visibleRows,
            ObservedRowCount = visibleRows.Count,
            Truncated = truncated,
            Correlation = request.Correlation
        };
    }

    // islevi: Katalog veya authorization failure'ini bos ve correlation-echo'lu projection sonucuna cevirir.
    private static ProjectionResult CreateResult(string outcomeCode, ProjectionRequest request)
        => new() { OutcomeCode = outcomeCode, Correlation = request.Correlation };
}
