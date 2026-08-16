using Microsoft.EntityFrameworkCore;
using Ptn.ApiContractChecker.Constants;
using Ptn.ApiContractChecker.Entities;
using Ptn.ApiContractChecker.Entities.Runs;
using Ptn.ApiContractChecker.Constants.Runs.Lookups;
using Ptn.ApiContractChecker.EntityFrameworkCore;
using Ptn.ApiContractChecker.Interface.Runs;
using Ptn.ApiContractChecker.Models.Runs;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;

namespace Ptn.ApiContractChecker.Repository.Runs;

// islevi: ContractCheckRun icin hafif baslik, tam detay, execution snapshot'i, stale recovery ve durum gecisi sorgularini kurar.
// sistemdeki gorevi: Owned findings JSON'unu liste/status yolundan dislayip tum run/snapshot LINQ ve gorunurlugunu persistence katmaninda tutar.
public class ContractCheckRunRepository : BaseRepository<ContractCheckRun>, IContractCheckRunRepository
{
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;
    private readonly IDataFilter<IPassivable> _passivableFilter;

    public ContractCheckRunRepository(
        IDbContextProvider<ApiContractCheckerDbContext> provider,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser,
        IDataFilter<IPassivable> passivableFilter)
        : base(provider)
    {
        _currentTenant = currentTenant;
        _currentUser = currentUser;
        _passivableFilter = passivableFilter;
    }

    // Tek run basligini owned findings govdesini projeksiyona almadan getirir.
    public async Task<ContractCheckRunHeaderModel?> FindHeaderAsync(Guid id)
    {
        using (_passivableFilter.Disable())
        {
            var query = await BuildHeaderQueryAsync();
            return await query.FirstOrDefaultAsync(run => run.Id == id);
        }
    }

    // Tek run detayini owned findings govdesini dahil eden ayri projeksiyonla getirir.
    public async Task<ContractCheckRunDetailModel?> FindDetailAsync(Guid id)
    {
        using (_passivableFilter.Disable())
        {
            var query = await BuildDetailQueryAsync();
            return await query.FirstOrDefaultAsync(run => run.Id == id);
        }
    }

    // Durum gecisi icin tenant ve host gorunurlugundeki aggregate'i owned govdesiyle getirir.
    public async Task<ContractCheckRun?> FindEntityAsync(Guid id)
    {
        var query = await BuildAccessibleRunQueryAsync();
        return await query.FirstOrDefaultAsync(run => run.Id == id);
    }

    // Run ile iki snapshot'in degismez icerigini tek tenant-kapsamli projection sorgusunda getirir.
    public async Task<ContractCheckSnapshotPairModel?> FindExecutionSnapshotPairAsync(Guid id)
    {
        var runs = await BuildAccessibleRunQueryAsync();
        var dbContext = await GetDbContextAsync();
        return await (
            from run in runs
            join baseSnapshot in dbContext.SpecSnapshots on run.BaseSnapshotId equals baseSnapshot.Id
            join baseContent in dbContext.SpecContents on baseSnapshot.SpecContentId equals baseContent.Id
            join targetSnapshot in dbContext.SpecSnapshots on run.TargetSnapshotId equals targetSnapshot.Id
            join targetContent in dbContext.SpecContents on targetSnapshot.SpecContentId equals targetContent.Id
            where run.Id == id
            select new ContractCheckSnapshotPairModel
            {
                BaseContent = baseContent.Content,
                TargetContent = targetContent.Content
            }).FirstOrDefaultAsync();
    }

    // StartedAt esigini asmis ve terminal zamani olmayan Running aggregate'leri tek sorguda getirir.
    public async Task<List<ContractCheckRun>> GetStaleRunningAsync(Guid runningStatusId, DateTime startedBefore)
    {
        var runs = await BuildAccessibleRunQueryAsync();
        return await runs
            .Where(run =>
                run.CheckRunStatusId == runningStatusId &&
                run.StartedAt < startedBefore &&
                run.CompletedAt == null)
            .ToListAsync();
    }

    // Run basliklarini dokuman/kaynak filtrelerinden sonra CreationTime ve Id azalan sirada sayfalar.
    public async Task<List<ContractCheckRunHeaderModel>> GetPagedHeadersAsync(
        int skipCount,
        int maxResultCount,
        Guid? specDocumentId,
        Guid? specSourceId)
    {
        using (_passivableFilter.Disable())
        {
            var query = await BuildFilteredHeaderQueryAsync(specDocumentId, specSourceId);
            return await query
                .OrderByDescending(run => run.CreationTime)
                .ThenByDescending(run => run.Id)
                .Skip(skipCount)
                .Take(maxResultCount)
                .ToListAsync();
        }
    }

    // Listeyle ayni dokuman/kaynak filtrelerinde gorulebilir run sayisini hesaplar.
    public async Task<long> GetHeaderCountAsync(Guid? specDocumentId, Guid? specSourceId)
    {
        using (_passivableFilter.Disable())
        {
            var query = await BuildFilteredRunQueryAsync(specDocumentId, specSourceId);
            return await query.LongCountAsync();
        }
    }

    // Owned JSON koleksiyonundan yalniz fingerprint alanini projekte eder.
    public async Task<List<string?>> GetFindingFingerprintsAsync(Guid id)
    {
        if (await RequiresSingleRunJsonFallbackAsync())
        {
            var body = await LoadSingleRunFindingsAsync(id);
            return body.Items.Select(finding => finding.Fingerprint).ToList();
        }

        var findings = await BuildFindingQueryAsync(id);
        return await findings.Select(finding => finding.Fingerprint).ToListAsync();
    }

    // Ayni base/target dokuman cifti icin en yakin eski Completed run'in fingerprintlerini projekte eder.
    public async Task<List<string?>?> FindPreviousCompletedFindingFingerprintsAsync(Guid id)
    {
        var previousRunId = await FindPreviousCompletedRunIdAsync(id);
        if (!previousRunId.HasValue)
        {
            return null;
        }

        return await GetFindingFingerprintsAsync(previousRunId.Value);
    }

    // Explicit referansi ayni gorunurluk, dokuman cifti, Completed durum ve kronolojik oncelik kurallariyla cozer.
    public async Task<List<string?>?> FindCompletedReferenceFindingFingerprintsAsync(
        Guid currentRunId,
        Guid referenceRunId)
    {
        var validReferenceId = await FindCompletedReferenceRunIdAsync(currentRunId, referenceRunId);
        return validReferenceId.HasValue
            ? await GetFindingFingerprintsAsync(validReferenceId.Value)
            : null;
    }

    // Tek run bulgularini filtre, kararli adres sirasi ve ABP sayfa degerleriyle projekte eder.
    public async Task<List<FindingReadModel>> GetPagedFindingsAsync(
        Guid id,
        int skipCount,
        int maxResultCount,
        string? severityCode,
        string? kindCode,
        string? path,
        string? schemaName,
        FindingFingerprintSelectionModel? fingerprintSelection)
    {
        if (await RequiresSingleRunJsonFallbackAsync())
        {
            var body = await LoadSingleRunFindingsAsync(id);
            var filtered = ApplyInMemoryFilters(
                body.Items, severityCode, kindCode, path, schemaName, fingerprintSelection);
            return filtered
                .OrderBy(finding => finding.Address.Path)
                .ThenBy(finding => finding.Address.HttpMethod)
                .ThenBy(finding => finding.Address.SchemaName)
                .ThenBy(finding => finding.Address.PropertyPath)
                .ThenBy(finding => finding.KindCode)
                .ThenBy(finding => finding.Fingerprint)
                .Skip(skipCount)
                .Take(maxResultCount)
                .Select(ProjectFinding)
                .ToList();
        }

        var query = await BuildFilteredFindingQueryAsync(
            id, severityCode, kindCode, path, schemaName, fingerprintSelection);
        return await query
            .OrderBy(finding => finding.Address.Path)
            .ThenBy(finding => finding.Address.HttpMethod)
            .ThenBy(finding => finding.Address.SchemaName)
            .ThenBy(finding => finding.Address.PropertyPath)
            .ThenBy(finding => finding.KindCode)
            .ThenBy(finding => finding.Fingerprint)
            .Skip(skipCount)
            .Take(maxResultCount)
            .Select(finding => new FindingReadModel
            {
                KindCode = finding.KindCode,
                SeverityCode = finding.SeverityCode,
                DirectionCode = finding.DirectionCode,
                Fingerprint = finding.Fingerprint,
                OldValue = finding.OldValue,
                NewValue = finding.NewValue,
                Address = new FindingAddressReadModel
                {
                    OperationId = finding.Address.OperationId,
                    HttpMethod = finding.Address.HttpMethod,
                    Path = finding.Address.Path,
                    SchemaName = finding.Address.SchemaName,
                    PropertyPath = finding.Address.PropertyPath,
                    ParameterName = finding.Address.ParameterName,
                    ResponseStatus = finding.Address.ResponseStatus,
                    MediaType = finding.Address.MediaType
                }
            })
            .ToListAsync();
    }

    // Sayfayla ayni owned JSON filtrelerinin toplam bulgu sayisini hesaplar.
    public async Task<long> GetFindingCountAsync(
        Guid id,
        string? severityCode,
        string? kindCode,
        string? path,
        string? schemaName,
        FindingFingerprintSelectionModel? fingerprintSelection)
    {
        if (await RequiresSingleRunJsonFallbackAsync())
        {
            var body = await LoadSingleRunFindingsAsync(id);
            return ApplyInMemoryFilters(
                    body.Items, severityCode, kindCode, path, schemaName, fingerprintSelection)
                .LongCount();
        }

        var query = await BuildFilteredFindingQueryAsync(
            id, severityCode, kindCode, path, schemaName, fingerprintSelection);
        return await query.LongCountAsync();
    }

    // SQLite owned JSON koleksiyon sorgusu APPLY gerektirdigi icin yalniz secili run'in govdesine geri cekilir.
    private async Task<bool> RequiresSingleRunJsonFallbackAsync()
    {
        var dbContext = await GetDbContextAsync();
        return dbContext.Database.ProviderName == ApiContractCheckerDatabaseConstants.SqliteProviderName;
    }

    // Geri cekilme yolunda tum run'lari degil yalniz kimligi verilen run'in findings kolonunu projekte eder.
    private async Task<ContractCheckFindings> LoadSingleRunFindingsAsync(Guid id)
    {
        var runs = await BuildAccessibleRunQueryAsync();
        return await runs
                   .AsNoTracking()
                   .Where(run => run.Id == id)
                   .Select(run => run.Findings)
                   .FirstOrDefaultAsync()
               ?? ContractCheckFindings.Empty();
    }

    // Geri cekilen tek run govdesinde tum istemci filtrelerini repository sinirinda uygular.
    private static IEnumerable<Finding> ApplyInMemoryFilters(
        IEnumerable<Finding> findings,
        string? severityCode,
        string? kindCode,
        string? path,
        string? schemaName,
        FindingFingerprintSelectionModel? selection)
    {
        if (severityCode is not null)
        {
            findings = findings.Where(finding => finding.SeverityCode == severityCode);
        }

        if (kindCode is not null)
        {
            findings = findings.Where(finding => finding.KindCode == kindCode);
        }

        if (path is not null)
        {
            findings = findings.Where(finding => finding.Address.Path?.Contains(path) == true);
        }

        if (schemaName is not null)
        {
            findings = findings.Where(finding => finding.Address.SchemaName == schemaName);
        }

        return ApplyInMemoryFingerprintSelection(findings, selection);
    }

    // Manager fingerprint secimini tek-run bellek koleksiyonuna uygular.
    private static IEnumerable<Finding> ApplyInMemoryFingerprintSelection(
        IEnumerable<Finding> findings,
        FindingFingerprintSelectionModel? selection)
    {
        if (selection is null)
        {
            return findings;
        }

        return selection.IncludeMissingFingerprint
            ? findings.Where(finding => finding.Fingerprint is null)
            : findings.Where(finding => finding.Fingerprint is not null &&
                                        selection.Fingerprints.Contains(finding.Fingerprint));
    }

    // Owned finding'i EF projectionuyla ayni duz read model sekline cevirir.
    private static FindingReadModel ProjectFinding(Finding finding)
    {
        return new FindingReadModel
        {
            KindCode = finding.KindCode,
            SeverityCode = finding.SeverityCode,
            DirectionCode = finding.DirectionCode,
            Fingerprint = finding.Fingerprint,
            OldValue = finding.OldValue,
            NewValue = finding.NewValue,
            Address = new FindingAddressReadModel
            {
                OperationId = finding.Address.OperationId,
                HttpMethod = finding.Address.HttpMethod,
                Path = finding.Address.Path,
                SchemaName = finding.Address.SchemaName,
                PropertyPath = finding.Address.PropertyPath,
                ParameterName = finding.Address.ParameterName,
                ResponseStatus = finding.Address.ResponseStatus,
                MediaType = finding.Address.MediaType
            }
        };
    }

    // Mevcut run'in dokuman ciftiyle eslesen en yakin eski Completed run kimligini bulur.
    private async Task<Guid?> FindPreviousCompletedRunIdAsync(Guid id)
    {
        var runs = await BuildAccessibleRunQueryAsync();
        var dbContext = await GetDbContextAsync();
        var currentDefinitions = BuildFindingReferenceDefinitions(runs, dbContext, id);
        var previousRuns =
            from definition in currentDefinitions
            from previous in runs
            join previousBase in dbContext.SpecSnapshots on previous.BaseSnapshotId equals previousBase.Id
            join previousTarget in dbContext.SpecSnapshots on previous.TargetSnapshotId equals previousTarget.Id
            join status in dbContext.CheckRunStatuses on previous.CheckRunStatusId equals status.Id
            where previous.Id != id &&
                  previous.CreationTime < definition.CreationTime &&
                  previousBase.SpecDocumentId == definition.BaseDocumentId &&
                  previousTarget.SpecDocumentId == definition.TargetDocumentId &&
                  status.Code == CheckRunStatusCodes.Completed
            orderby previous.CompletedAt descending, previous.CreationTime descending, previous.Id descending
            select (Guid?)previous.Id;
        return await previousRuns.FirstOrDefaultAsync();
    }

    // Belirtilen reference run'i mevcut run'in ayni base/target dokuman tanimina bagli eski Completed run olarak secer.
    private async Task<Guid?> FindCompletedReferenceRunIdAsync(Guid currentRunId, Guid referenceRunId)
    {
        var runs = await BuildAccessibleRunQueryAsync();
        var dbContext = await GetDbContextAsync();
        var currentDefinitions = BuildFindingReferenceDefinitions(runs, dbContext, currentRunId);
        var references =
            from definition in currentDefinitions
            from reference in runs
            join referenceBase in dbContext.SpecSnapshots on reference.BaseSnapshotId equals referenceBase.Id
            join referenceTarget in dbContext.SpecSnapshots on reference.TargetSnapshotId equals referenceTarget.Id
            join status in dbContext.CheckRunStatuses on reference.CheckRunStatusId equals status.Id
            where reference.Id == referenceRunId &&
                  reference.Id != currentRunId &&
                  reference.CreationTime < definition.CreationTime &&
                  referenceBase.SpecDocumentId == definition.BaseDocumentId &&
                  referenceTarget.SpecDocumentId == definition.TargetDocumentId &&
                  status.Code == CheckRunStatusCodes.Completed
            select (Guid?)reference.Id;
        return await references.FirstOrDefaultAsync();
    }

    // Mevcut run'in base/target dokuman tanimini owned findings kolonuna dokunmadan projekte eder.
    private static IQueryable<FindingReferenceDefinition> BuildFindingReferenceDefinitions(
        IQueryable<ContractCheckRun> runs,
        ApiContractCheckerDbContext dbContext,
        Guid runId)
        => from current in runs
           join currentBase in dbContext.SpecSnapshots on current.BaseSnapshotId equals currentBase.Id
           join currentTarget in dbContext.SpecSnapshots on current.TargetSnapshotId equals currentTarget.Id
           where current.Id == runId
           select new FindingReferenceDefinition
           {
               BaseDocumentId = currentBase.SpecDocumentId,
               TargetDocumentId = currentTarget.SpecDocumentId,
               CreationTime = current.CreationTime
           };

    // Tek run owned JSON koleksiyonuna tenant/host gorunurlugu altinda erisir.
    private async Task<IQueryable<Finding>> BuildFindingQueryAsync(Guid id)
    {
        var runs = await BuildAccessibleRunQueryAsync();
        return BuildFindingQuery(runs, id);
    }

    // Tek run owned JSON koleksiyonunu provider tarafinda acan ortak sorgu seklini test edilebilir tutar.
    internal static IQueryable<Finding> BuildFindingQuery(
        IQueryable<ContractCheckRun> runs,
        Guid id)
        => runs.Where(run => run.Id == id).SelectMany(run => run.Findings.Items);

    // Bulgu alan filtreleriyle manager tarafindan secilmis fingerprint kumesini repository LINQ'ine uygular.
    private async Task<IQueryable<Finding>> BuildFilteredFindingQueryAsync(
        Guid id,
        string? severityCode,
        string? kindCode,
        string? path,
        string? schemaName,
        FindingFingerprintSelectionModel? fingerprintSelection)
    {
        var findings = await BuildFindingQueryAsync(id);
        if (severityCode is not null)
        {
            findings = findings.Where(finding => finding.SeverityCode == severityCode);
        }

        if (kindCode is not null)
        {
            findings = findings.Where(finding => finding.KindCode == kindCode);
        }

        if (path is not null)
        {
            findings = findings.Where(finding => finding.Address.Path != null && finding.Address.Path.Contains(path));
        }

        if (schemaName is not null)
        {
            findings = findings.Where(finding => finding.Address.SchemaName == schemaName);
        }

        return ApplyFingerprintSelection(findings, fingerprintSelection);
    }

    // Manager kararini null veya IN filtresi olarak provider sorgusuna cevirir.
    private static IQueryable<Finding> ApplyFingerprintSelection(
        IQueryable<Finding> findings,
        FindingFingerprintSelectionModel? selection)
    {
        if (selection is null)
        {
            return findings;
        }

        if (selection.IncludeMissingFingerprint)
        {
            return findings.Where(finding => finding.Fingerprint == null);
        }

        var fingerprints = selection.Fingerprints.ToArray();
        return findings.Where(finding => finding.Fingerprint != null && fingerprints.Contains(finding.Fingerprint));
    }

    // Filtrelenmis run sorgusunu findings haric scalar baslik kolonlarina projekte eder.
    private async Task<IQueryable<ContractCheckRunHeaderModel>> BuildFilteredHeaderQueryAsync(
        Guid? specDocumentId,
        Guid? specSourceId)
    {
        var runs = await BuildFilteredRunQueryAsync(specDocumentId, specSourceId);
        return await ProjectHeadersAsync(runs);
    }

    // Opsiyonel dokuman ve kaynak filtrelerini iki snapshot tarafina da veritabani sorgusu olarak uygular.
    private async Task<IQueryable<ContractCheckRun>> BuildFilteredRunQueryAsync(
        Guid? specDocumentId,
        Guid? specSourceId)
    {
        var runs = await BuildAccessibleRunQueryAsync();
        var dbContext = await GetDbContextAsync();

        if (specDocumentId.HasValue)
        {
            var documentId = specDocumentId.Value;
            runs = runs.Where(run => dbContext.SpecSnapshots.Any(snapshot =>
                (snapshot.Id == run.BaseSnapshotId || snapshot.Id == run.TargetSnapshotId) &&
                snapshot.SpecDocumentId == documentId));
        }

        if (specSourceId.HasValue)
        {
            var sourceId = specSourceId.Value;
            runs = runs.Where(run =>
                (from snapshot in dbContext.SpecSnapshots
                 join document in dbContext.SpecDocuments
                     on snapshot.SpecDocumentId equals document.Id
                 where (snapshot.Id == run.BaseSnapshotId || snapshot.Id == run.TargetSnapshotId) &&
                       document.SpecSourceId == sourceId
                 select snapshot.Id).Any());
        }

        return runs;
    }

    // Gorulebilir run sorgusunu status kodu dahil hafif baslik modeline projekte eder.
    private async Task<IQueryable<ContractCheckRunHeaderModel>> BuildHeaderQueryAsync()
    {
        var runs = await BuildAccessibleRunQueryAsync();
        return await ProjectHeadersAsync(runs);
    }

    // Verilen run sorgusuna yalniz baslik kolonlari ve status kodu projeksiyonunu uygular.
    private async Task<IQueryable<ContractCheckRunHeaderModel>> ProjectHeadersAsync(
        IQueryable<ContractCheckRun> runs)
    {
        var dbContext = await GetDbContextAsync();
        return
            from run in runs
            join status in dbContext.CheckRunStatuses
                on run.CheckRunStatusId equals status.Id
            select new ContractCheckRunHeaderModel
            {
                Id = run.Id,
                BaseSnapshotId = run.BaseSnapshotId,
                TargetSnapshotId = run.TargetSnapshotId,
                CheckRunStatusId = run.CheckRunStatusId,
                StatusCode = status.Code,
                StartedAt = run.StartedAt,
                CompletedAt = run.CompletedAt,
                ErrorMessage = run.ErrorMessage,
                BreakingCount = run.BreakingCount,
                NonBreakingCount = run.NonBreakingCount,
                DocsOnlyCount = run.DocsOnlyCount,
                CreationTime = run.CreationTime
            };
    }

    // Gorulebilir run sorgusunu status kodu ve tam findings govdesiyle detay modeline projekte eder.
    private async Task<IQueryable<ContractCheckRunDetailModel>> BuildDetailQueryAsync()
    {
        var runs = (await BuildAccessibleRunQueryAsync()).AsNoTracking();
        var dbContext = await GetDbContextAsync();
        return
            from run in runs
            join status in dbContext.CheckRunStatuses
                on run.CheckRunStatusId equals status.Id
            select new ContractCheckRunDetailModel
            {
                Id = run.Id,
                BaseSnapshotId = run.BaseSnapshotId,
                TargetSnapshotId = run.TargetSnapshotId,
                CheckRunStatusId = run.CheckRunStatusId,
                StatusCode = status.Code,
                StartedAt = run.StartedAt,
                CompletedAt = run.CompletedAt,
                ErrorMessage = run.ErrorMessage,
                BreakingCount = run.BreakingCount,
                NonBreakingCount = run.NonBreakingCount,
                DocsOnlyCount = run.DocsOnlyCount,
                CreationTime = run.CreationTime,
                Findings = run.Findings
            };
    }

    // ABP tenant filtresini host kullanicisinin kendi ve sistem run kayitlariyla tamamlar.
    private async Task<IQueryable<ContractCheckRun>> BuildAccessibleRunQueryAsync()
    {
        var query = await GetQueryableAsync();
        if (_currentTenant.Id.HasValue)
        {
            return query;
        }

        var userId = _currentUser.Id;

        // Kullanici yoksa cagiran HTTP istegi degil sistemdir (arka plan isi): host kullanici filtresi
        // uygulanmaz. Aksi halde filtre "CreatorId == null"a indirgenir, bir kullanicinin tetikledigi run
        // job'a hic gorunmez ve run sonsuza kadar Pending kalir (KBP-617 canli tur bulgusu).
        // Anonim HTTP yolu yoktur: tum check endpointleri Checks.View izni ister.
        if (userId is null)
        {
            return query;
        }

        return query.Where(run => run.CreatorId == null || run.CreatorId == userId);
    }

    // islevi: Finding reference sorgusunun uc iliskili tanim alanini tekrar kullanilabilir typed projeksiyonda tasir.
    private sealed class FindingReferenceDefinition
    {
        public Guid BaseDocumentId { get; init; }
        public Guid TargetDocumentId { get; init; }
        public DateTime CreationTime { get; init; }
    }
}
