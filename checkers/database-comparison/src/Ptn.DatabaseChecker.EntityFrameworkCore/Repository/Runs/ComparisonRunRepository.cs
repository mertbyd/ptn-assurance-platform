using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ptn.DatabaseChecker.Entities;
using Ptn.DatabaseChecker.Entities.Runs;
using Ptn.DatabaseChecker.EntityFrameworkCore;
using Ptn.DatabaseChecker.Interface.Runs;
using Ptn.DatabaseChecker.Models.Runs;
using Ptn.DatabaseChecker.Constants;
using Ptn.DatabaseChecker.Constants.Comparison;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;

namespace Ptn.DatabaseChecker.Repository.Runs;

// islevi: ComparisonRun sorgularini hem hafif header projeksiyonuyla hem tam-detay Include'lariyla calistirir.
// sistemdeki gorevi: Liste/get-header owned jsonb (ScopeSnapshot/Findings/Reports) cekmeden projeksiyonla; detay okumasi tam entity'yi navigation ve owned kolonlariyla getirir. En yeni run en ustte doner.
public class ComparisonRunRepository : BaseRepository<ComparisonRun>, IComparisonRunRepository
{
    private readonly IDataFilter<IPassivable> _passivableDataFilter;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;

    public ComparisonRunRepository(
        IDbContextProvider<DatabaseCheckerDbContext> dbContextProvider,
        IDataFilter<IPassivable> passivableDataFilter,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser)
        : base(dbContextProvider)
    {
        _passivableDataFilter = passivableDataFilter;
        _currentTenant = currentTenant;
        _currentUser = currentUser;
    }

    public async Task<ComparisonRun?> FindWithDetailsAsync(Guid id)
    {
        using (_passivableDataFilter.Disable())
        {
            var query = await BuildAccessibleQueryAsync();
            return await query
                .Include(x => x.Definition)
                .Include(x => x.SourceConnection).ThenInclude(connection => connection.Engine)
                .Include(x => x.TargetConnection).ThenInclude(connection => connection.Engine)
                .Include(x => x.ComparisonType)
                .Include(x => x.Status)
                .FirstOrDefaultAsync(x => x.Id == id);
        }
    }

    public async Task<ComparisonRunHeaderModel?> FindHeaderAsync(Guid id)
    {
        using (_passivableDataFilter.Disable())
        {
            var query = await BuildAccessibleQueryAsync();
            return await ProjectHeaders(query).FirstOrDefaultAsync(x => x.Id == id);
        }
    }

    public async Task<List<ComparisonRunHeaderModel>> GetPagedHeadersAsync(Guid? comparisonDefinitionId, int skipCount, int maxResultCount)
    {
        using (_passivableDataFilter.Disable())
        {
            var query = await BuildAccessibleQueryAsync();
            return await ProjectHeaders(query)
                .WhereIf(comparisonDefinitionId.HasValue, x => x.ComparisonDefinitionId == comparisonDefinitionId!.Value)
                .OrderByDescending(x => x.CreationTime)
                .Skip(skipCount)
                .Take(maxResultCount)
                .ToListAsync();
        }
    }

    public async Task<List<ComparisonRunHeaderModel>> GetHeadersByIdsAsync(List<Guid> ids)
    {
        using (_passivableDataFilter.Disable())
        {
            var query = await BuildAccessibleQueryAsync();
            return await ProjectHeaders(query)
                .Where(x => ids.Contains(x.Id))
                .ToListAsync();
        }
    }

    public async Task<long> GetCountByDefinitionAsync(Guid? comparisonDefinitionId)
    {
        var query = await BuildAccessibleQueryAsync();
        return await query
            .WhereIf(comparisonDefinitionId.HasValue, x => x.ComparisonDefinitionId == comparisonDefinitionId!.Value)
            .LongCountAsync();
    }

    /// <summary>
    /// Uc owned JSON bulgu ailesini filtreleyip sinirli sunucu pencerelerinden tek sayfa uretir.
    /// </summary>
    public virtual async Task<FindingPageModel> GetFindingsAsync(
        Guid id,
        FindingQueryModel input,
        CancellationToken cancellationToken = default)
    {
        using (_passivableDataFilter.Disable())
        {
            var dbContext = await GetDbContextAsync();
            using var activity = DatabaseCheckerTelemetry.StartActivity(
                DatabaseCheckerTelemetryConstants.Activities.FindingsQuery,
                dbContext.Database.ProviderName,
                dbContext.Database.GetDbConnection().Database);
            var page = await QueryFindingsAsync(id, input, cancellationToken);
            activity.SetOutcomeCode(DatabaseCheckerTelemetryConstants.Outcomes.Completed);
            activity.SetAttemptCount(ResolveFindingQueryRoundTripCount(input));
            return page;
        }
    }

    // islevi: Ortak filtreleri uc JSON ailesine uygular, referans fingerprintlerini cikarir ve tek global sayfa kurar.
    private async Task<FindingPageModel> QueryFindingsAsync(
        Guid id,
        FindingQueryModel input,
        CancellationToken cancellationToken)
    {
        var runs = (await BuildAccessibleQueryAsync()).Where(run => run.Id == id);
        var schema = ApplyFindingFilters(BuildSchemaFindingQuery(runs), input);
        var migrations = ApplyFindingFilters(BuildMigrationFindingQuery(runs), input);
        var data = ApplyFindingFilters(BuildDataFindingQuery(runs), input);
        var referenceFingerprints = await ReadReferenceFingerprintsAsync(input.SinceRunId, cancellationToken);
        schema = ApplySinceRunFilter(schema, referenceFingerprints);
        migrations = ApplySinceRunFilter(migrations, referenceFingerprints);
        data = ApplySinceRunFilter(data, referenceFingerprints);
        var totalCount = await CountFindingsAsync(schema, migrations, data, cancellationToken);
        var items = await ReadFindingWindowAsync(
            schema, migrations, data, ResolveWindowCount(input), cancellationToken);
        return BuildFindingPage(totalCount, items, input);
    }

    // islevi: Telemetriye yalniz repository icinde gerceklesen count/window ve opsiyonel referans projection tur sayisini yazar.
    private static int ResolveFindingQueryRoundTripCount(FindingQueryModel input)
        => input.SinceRunId.HasValue
            ? ComparisonRunConsts.FindingSinceQueryRoundTripCount
            : ComparisonRunConsts.FindingQueryRoundTripCount;

    /// <summary>
    /// Ayni tenant/definition icindeki en yakin onceki Completed run'in fingerprintlerini okur.
    /// </summary>
    public virtual async Task<IReadOnlyCollection<string>?> FindPreviousCompletedFindingFingerprintsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        using (_passivableDataFilter.Disable())
        {
            var runs = await BuildAccessibleQueryAsync();
            var previousRunId = await runs
                .Where(reference =>
                    reference.Id != id &&
                    reference.Status.Code == ComparisonRunStatusCodes.Completed &&
                    runs.Any(current =>
                        current.Id == id &&
                        current.ComparisonDefinitionId.HasValue &&
                        current.ComparisonDefinitionId == reference.ComparisonDefinitionId &&
                        reference.CreationTime < current.CreationTime))
                .OrderByDescending(reference => reference.CreationTime)
                .Select(reference => (Guid?)reference.Id)
                .FirstOrDefaultAsync(cancellationToken);
            if (!previousRunId.HasValue)
            {
                return null;
            }

            // Owned JSON ailelerinin SelectMany projeksiyonu SQL APPLY ister; tek referans satiri
            // icin detay okumasi hem saglayici bagimsizdir hem de ayni tek sorguda kalir.
            var previousRun = await FindWithDetailsAsync(previousRunId.Value);
            return previousRun is null ? null : CollectFingerprints(previousRun);
        }
    }

    // islevi: Tek run'in uc owned bulgu ailesindeki fingerprintlerini ordinal kumede toplar; legacy null degerleri disarida birakir.
    private static IReadOnlyCollection<string> CollectFingerprints(ComparisonRun run)
    {
        var result = new HashSet<string>(StringComparer.Ordinal);
        if (run.Findings is null)
        {
            return result;
        }

        AddFingerprints(result, run.Findings.SchemaDifferences.Select(finding => finding.Fingerprint));
        AddFingerprints(result, run.Findings.MigrationDifferences.Select(finding => finding.Fingerprint));
        AddFingerprints(result, run.Findings.DataDifferences.Select(finding => finding.Fingerprint));
        return result;
    }

    // islevi: Null olmayan fingerprintleri hedef kumeye ekler.
    private static void AddFingerprints(HashSet<string> target, IEnumerable<string?> fingerprints)
    {
        foreach (var fingerprint in fingerprints)
        {
            if (fingerprint is not null)
            {
                target.Add(fingerprint);
            }
        }
    }

    /// <summary>
    /// Referans run'i ayni tenant/definition, Completed durum ve kronolojik oncelik kurallariyla sorgular.
    /// </summary>
    public virtual async Task<bool> IsValidCompletedReferenceRunAsync(
        Guid currentRunId,
        Guid referenceRunId,
        CancellationToken cancellationToken = default)
    {
        using (_passivableDataFilter.Disable())
        {
            var runs = await BuildAccessibleQueryAsync();
            return await runs.AnyAsync(reference =>
                reference.Id == referenceRunId &&
                reference.Id != currentRunId &&
                reference.Status.Code == ComparisonRunStatusCodes.Completed &&
                runs.Any(current =>
                    current.Id == currentRunId &&
                    current.ComparisonDefinitionId.HasValue &&
                    current.ComparisonDefinitionId == reference.ComparisonDefinitionId &&
                    reference.CreationTime < current.CreationTime),
                cancellationToken);
        }
    }

    // islevi: Yapisal JSON elemanlarini adres ve delta alanlariyla ortak bulgu modeline projekte eder.
    internal static IQueryable<FindingReadModel> BuildSchemaFindingQuery(IQueryable<ComparisonRun> runs)
        => runs.SelectMany(run => run.Findings.SchemaDifferences, (run, difference) => new FindingReadModel
        {
            Fingerprint = difference.Fingerprint,
            Address = new FindingAddressReadModel
            {
                SourceEngineCode = run.SourceConnection.Engine.Code,
                TargetEngineCode = run.TargetConnection.Engine.Code,
                SchemaName = difference.SchemaName,
                ObjectTypeCode = difference.ObjectTypeCode,
                ObjectName = difference.ObjectName,
                ChildName = difference.ChildName
            },
            SeverityCode = difference.SeverityCode,
            KindCode = difference.KindCode,
            ObjectTypeCode = difference.ObjectTypeCode,
            SchemaName = difference.SchemaName,
            ObjectName = difference.ObjectName,
            TableName = difference.ObjectName,
            ChildName = difference.ChildName,
            ConfidenceCode = difference.ConfidenceCode,
            SourceValue = difference.SourceDefinition,
            TargetValue = difference.TargetDefinition,
            SourceRowCount = null,
            TargetRowCount = null,
            RowCountDifference = null,
            ChangeSummary = difference.ChangeSummary
        });

    // islevi: Migration JSON elemanlarini defter adresi ve surum deltasiyla ortak bulgu modeline projekte eder.
    private static IQueryable<FindingReadModel> BuildMigrationFindingQuery(IQueryable<ComparisonRun> runs)
        => runs.SelectMany(run => run.Findings.MigrationDifferences, (run, difference) => new FindingReadModel
        {
            Fingerprint = difference.Fingerprint,
            Address = new FindingAddressReadModel
            {
                SourceEngineCode = run.SourceConnection.Engine.Code,
                TargetEngineCode = run.TargetConnection.Engine.Code,
                SchemaName = difference.SourceSchemaName ?? difference.TargetSchemaName,
                ObjectTypeCode = SchemaObjectTypeCodes.Migration,
                ObjectName = difference.MigrationId,
                ChildName = null
            },
            SeverityCode = difference.SeverityCode,
            KindCode = difference.KindCode,
            ObjectTypeCode = SchemaObjectTypeCodes.Migration,
            SchemaName = difference.SourceSchemaName ?? difference.TargetSchemaName,
            ObjectName = difference.MigrationId,
            TableName = null,
            ChildName = null,
            ConfidenceCode = null,
            SourceValue = difference.SourceProductVersion,
            TargetValue = difference.TargetProductVersion,
            SourceRowCount = null,
            TargetRowCount = null,
            RowCountDifference = null,
            ChangeSummary = null
        });

    // islevi: Veri JSON elemanlarini tablo adresi, sayim ve hash deltasiyla ortak bulgu modeline projekte eder.
    private static IQueryable<FindingReadModel> BuildDataFindingQuery(IQueryable<ComparisonRun> runs)
        => runs.SelectMany(run => run.Findings.DataDifferences, (run, difference) => new FindingReadModel
        {
            Fingerprint = difference.Fingerprint,
            Address = new FindingAddressReadModel
            {
                SourceEngineCode = run.SourceConnection.Engine.Code,
                TargetEngineCode = run.TargetConnection.Engine.Code,
                SchemaName = difference.SchemaName,
                ObjectTypeCode = SchemaObjectTypeCodes.Table,
                ObjectName = difference.TableName,
                ChildName = null
            },
            SeverityCode = difference.SeverityCode,
            KindCode = difference.KindCode,
            ObjectTypeCode = SchemaObjectTypeCodes.Table,
            SchemaName = difference.SchemaName,
            ObjectName = difference.TableName,
            TableName = difference.TableName,
            ChildName = null,
            ConfidenceCode = null,
            SourceValue = difference.SourceHash,
            TargetValue = difference.TargetHash,
            SourceRowCount = difference.SourceRowCount,
            TargetRowCount = difference.TargetRowCount,
            RowCountDifference = difference.RowCountDifference,
            ChangeSummary = null
        });

    // islevi: Uc bulgu ailesinin filtreli sayilarini sunucuda hesaplayip tek toplamda birlestirir.
    private static async Task<long> CountFindingsAsync(
        IQueryable<FindingReadModel> schema,
        IQueryable<FindingReadModel> migrations,
        IQueryable<FindingReadModel> data,
        CancellationToken cancellationToken)
        => await schema.LongCountAsync(cancellationToken) +
           await migrations.LongCountAsync(cancellationToken) +
           await data.LongCountAsync(cancellationToken);

    // islevi: Her JSON ailesinden yalniz global sayfa icin gerekli ust pencereyi sunucuda sirali okur.
    private static async Task<List<FindingReadModel>> ReadFindingWindowAsync(
        IQueryable<FindingReadModel> schema,
        IQueryable<FindingReadModel> migrations,
        IQueryable<FindingReadModel> data,
        int windowCount,
        CancellationToken cancellationToken)
    {
        var items = await ReadFamilyWindowAsync(schema, windowCount, cancellationToken);
        items.AddRange(await ReadFamilyWindowAsync(migrations, windowCount, cancellationToken));
        items.AddRange(await ReadFamilyWindowAsync(data, windowCount, cancellationToken));
        return items;
    }

    // islevi: Tek JSON ailesini fingerprint sirasinda sunucu LIMIT'i ile okur.
    private static Task<List<FindingReadModel>> ReadFamilyWindowAsync(
        IQueryable<FindingReadModel> findings,
        int windowCount,
        CancellationToken cancellationToken)
        => findings
            .OrderBy(finding => finding.Fingerprint)
            .Take(windowCount)
            .ToListAsync(cancellationToken);

    // islevi: Uc sinirli pencereyi kararli sirada birlestirip istenen global sayfayi kurar.
    private static FindingPageModel BuildFindingPage(
        long totalCount,
        List<FindingReadModel> items,
        FindingQueryModel input)
        => new()
        {
            TotalCount = totalCount,
            Items = items
                .OrderBy(finding => finding.Fingerprint, StringComparer.Ordinal)
                .ThenBy(finding => finding.ObjectTypeCode, StringComparer.Ordinal)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToList()
        };

    // islevi: Global Skip+Take penceresini overflow olmadan tek aile sunucu limitine cevirir.
    private static int ResolveWindowCount(FindingQueryModel input)
        => (int)Math.Min(
            int.MaxValue,
            Math.Max(0L, input.SkipCount) + Math.Max(0L, input.MaxResultCount));

    // islevi: Opsiyonel MCP filtrelerini ortak bulgu projection'ina materialization oncesinde uygular.
    internal static IQueryable<FindingReadModel> ApplyFindingFilters(
        IQueryable<FindingReadModel> findings,
        FindingQueryModel input)
    {
        findings = Filter(findings, input.SeverityCode, finding => finding.SeverityCode);
        findings = Filter(findings, input.KindCode, finding => finding.KindCode);
        findings = Filter(findings, input.ObjectTypeCode, finding => finding.ObjectTypeCode);
        findings = Filter(findings, input.SchemaName, finding => finding.SchemaName);
        findings = Filter(findings, input.TableName, finding => finding.TableName);
        if (input.Fingerprints.Count == 0)
        {
            return findings;
        }

        var fingerprints = input.Fingerprints.ToArray();
        return findings.Where(finding =>
            finding.Fingerprint != null && fingerprints.Contains(finding.Fingerprint));
    }

    // islevi: SinceRunId verilirse referans run'dan yalniz bilinen fingerprint projection'larini okur.
    private async Task<IReadOnlyCollection<string>?> ReadReferenceFingerprintsAsync(
        Guid? referenceRunId,
        CancellationToken cancellationToken)
    {
        if (!referenceRunId.HasValue)
        {
            return null;
        }

        var runs = (await BuildAccessibleQueryAsync()).Where(run => run.Id == referenceRunId.Value);
        var fingerprints = await ReadFingerprintsAsync(runs, cancellationToken);
        return fingerprints.Distinct(StringComparer.Ordinal).ToArray();
    }

    // islevi: Uc owned JSON ailesinden yalniz fingerprint scalar alanlarini projekte eder; bulgu govdelerini cekmez.
    private static async Task<List<string>> ReadFingerprintsAsync(
        IQueryable<ComparisonRun> runs,
        CancellationToken cancellationToken)
    {
        var result = await runs.SelectMany(run => run.Findings.SchemaDifferences)
            .Where(finding => finding.Fingerprint != null)
            .Select(finding => finding.Fingerprint!)
            .ToListAsync(cancellationToken);
        result.AddRange(await runs.SelectMany(run => run.Findings.MigrationDifferences)
            .Where(finding => finding.Fingerprint != null)
            .Select(finding => finding.Fingerprint!)
            .ToListAsync(cancellationToken));
        result.AddRange(await runs.SelectMany(run => run.Findings.DataDifferences)
            .Where(finding => finding.Fingerprint != null)
            .Select(finding => finding.Fingerprint!)
            .ToListAsync(cancellationToken));
        return result;
    }

    // islevi: Referansta bilinen fingerprintleri cikartir; legacy null fingerprintleri New saymaz.
    internal static IQueryable<FindingReadModel> ApplySinceRunFilter(
        IQueryable<FindingReadModel> findings,
        IReadOnlyCollection<string>? referenceFingerprints)
    {
        if (referenceFingerprints is null)
        {
            return findings;
        }

        var known = referenceFingerprints.ToArray();
        return findings.Where(finding =>
            finding.Fingerprint != null && !known.Contains(finding.Fingerprint));
    }

    // islevi: Dolu string filtresini secilen projection alanina esitlik olarak uygular.
    private static IQueryable<FindingReadModel> Filter(
        IQueryable<FindingReadModel> findings,
        string? value,
        System.Linq.Expressions.Expression<Func<FindingReadModel, string?>> selector)
        => string.IsNullOrWhiteSpace(value)
            ? findings
            : findings.Where(BuildEquality(selector, value));

    // islevi: Secilen string alan icin EF tarafindan cevrilebilir esitlik ifadesi kurar.
    private static System.Linq.Expressions.Expression<Func<FindingReadModel, bool>> BuildEquality(
        System.Linq.Expressions.Expression<Func<FindingReadModel, string?>> selector,
        string value)
    {
        var body = System.Linq.Expressions.Expression.Equal(
            selector.Body,
            System.Linq.Expressions.Expression.Constant(value, typeof(string)));
        return System.Linq.Expressions.Expression.Lambda<Func<FindingReadModel, bool>>(body, selector.Parameters);
    }

    // Run satirini yalnizca header kolonlari + companion adlara projekte eder; owned jsonb kolonlari (ScopeSnapshot/Findings/Reports) SELECT'e girmez.
    // Nullable tarif LEFT JOIN, zorunlu baglanti/mod/durum INNER JOIN olarak EF tarafindan uretilir (is bir kez yapilir).
    private static IQueryable<ComparisonRunHeaderModel> ProjectHeaders(IQueryable<ComparisonRun> query)
    {
        return query.Select(run => new ComparisonRunHeaderModel
        {
            Id = run.Id,
            ComparisonDefinitionId = run.ComparisonDefinitionId,
            ComparisonDefinitionName = run.Definition != null ? run.Definition.Name : null,
            SourceConnectionId = run.SourceConnectionId,
            SourceConnectionName = run.SourceConnection.Name,
            TargetConnectionId = run.TargetConnectionId,
            TargetConnectionName = run.TargetConnection.Name,
            ComparisonTypeId = run.ComparisonTypeId,
            ComparisonTypeCode = run.ComparisonType.Code,
            ComparisonTypeName = run.ComparisonType.Name,
            StatusId = run.StatusId,
            StatusCode = run.Status.Code,
            StatusName = run.Status.Name,
            StartedAt = run.StartedAt,
            CompletedAt = run.CompletedAt,
            ErrorMessage = run.ErrorMessage,
            SchemaDifferenceCount = run.SchemaDifferenceCount,
            DataDifferenceCount = run.DataDifferenceCount,
            MigrationDifferenceCount = run.MigrationDifferenceCount,
            CreationTime = run.CreationTime
        });
    }

    // ABP tenant filtresini host kullanicisinin kendi ve sistem run kayitlariyla tamamlar.
    private async Task<IQueryable<ComparisonRun>> BuildAccessibleQueryAsync()
    {
        var query = await GetQueryableAsync();
        if (_currentTenant.Id.HasValue)
        {
            return query;
        }

        var userId = _currentUser.Id;
        if (userId is null)
        {
            // Background job kullanici claim'i tasimaz; tenant filtresi yine aktiftir.
            return query;
        }

        return query.Where(run => run.CreatorId == null || run.CreatorId == userId);
    }
}
