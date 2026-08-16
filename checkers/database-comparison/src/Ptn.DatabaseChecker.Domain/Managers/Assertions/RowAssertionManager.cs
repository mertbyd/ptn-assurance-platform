using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Constants.Comparison.Assertions;
using Ptn.DatabaseChecker.Constants;
using Ptn.DatabaseChecker.Entities.Connections;
using Ptn.DatabaseChecker.ExceptionCodes;
using Ptn.DatabaseChecker.Managers.Comparison;
using Ptn.DatabaseChecker.Models.Assertions;
using Ptn.DatabaseChecker.Models.Comparison;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Services;
using Volo.Abp.Timing;

namespace Ptn.DatabaseChecker.Managers.Assertions;

// islevi: Tek satir/count/absence assertion'ini "dogrula -> anahtari coz -> oku -> degerlendir -> sonuc kur" akisiyla calistirir.
// sistemdeki gorevi: Test Module oracle'inin katalog guvenligi, polling, matcher, outcome ve redaction kararlarini tek cekirdekte tutar; provider SQL'i mevcut repository omurgasinda kalir.
public class RowAssertionManager : DomainService
{
    private readonly DatabaseDataComparisonManager _dataManager;
    private readonly ValueMatcherEvaluator _matcher;
    private readonly AssertionSettingsResolver _settingsResolver;
    private readonly ValueRetentionPolicyResolver _retentionPolicyResolver;
    private readonly FindingValueRedactor _redactor;
    private readonly IClock _clock;

    // islevi: Assertion cekirdegini veri okuyucu, saf matcher, setting, redaction ve ABP saatiyle kurar.
    public RowAssertionManager(
        DatabaseDataComparisonManager dataManager,
        ValueMatcherEvaluator matcher,
        AssertionSettingsResolver settingsResolver,
        ValueRetentionPolicyResolver retentionPolicyResolver,
        FindingValueRedactor redactor,
        IClock clock)
    {
        _dataManager = dataManager;
        _matcher = matcher;
        _settingsResolver = settingsResolver;
        _retentionPolicyResolver = retentionPolicyResolver;
        _redactor = redactor;
        _clock = clock;
    }

    // islevi: Row endpoint semantigini Exactly(1) olarak uygulayip ortak assertion cekirdegini calistirir.
    public virtual Task<RowAssertionResult> AssertRowAsync(
        DatabaseConnection connection,
        RowAssertionRequest request,
        CancellationToken cancellationToken = default)
    {
        request.Cardinality.KindCode = CardinalityKindCodes.Exactly;
        request.Cardinality.ExpectedCount = 1;
        return AssertAsync(connection, request, cancellationToken);
    }

    // islevi: Count endpointini satir degeri okumadan yalniz cardinality sonucu uretecek sekilde calistirir.
    public virtual Task<RowAssertionResult> AssertCountAsync(
        DatabaseConnection connection,
        RowAssertionRequest request,
        CancellationToken cancellationToken = default)
    {
        request.Expectations.Clear();
        request.IncludeRowOnFailure = false;
        return AssertAsync(connection, request, cancellationToken);
    }

    // islevi: Absent endpoint semantigini None olarak uygulayip ortak assertion cekirdegini calistirir.
    public virtual Task<RowAssertionResult> AssertAbsentAsync(
        DatabaseConnection connection,
        RowAssertionRequest request,
        CancellationToken cancellationToken = default)
    {
        request.Cardinality.KindCode = CardinalityKindCodes.None;
        request.Cardinality.ExpectedCount = 0;
        request.Expectations.Clear();
        request.IncludeRowOnFailure = false;
        return AssertAsync(connection, request, cancellationToken);
    }

    // islevi: Batch limitini uygular, baglantilari kimlige gore esler ve tum assertion sonuclarini sirayla uretir.
    public virtual async Task<List<RowAssertionResult>> AssertBatchAsync(
        List<DatabaseConnection> connections,
        List<RowAssertionRequest> requests,
        CancellationToken cancellationToken = default)
    {
        var firstConnection = connections.FirstOrDefault();
        using var activity = DatabaseCheckerTelemetry.StartActivity(
            DatabaseCheckerTelemetryConstants.Activities.AssertBatch,
            firstConnection?.Engine.Code,
            firstConnection?.DatabaseName);
        await EnsureBatchSizeAsync(requests.Count);
        var connectionsById = connections.ToDictionary(connection => connection.Id);
        var results = await ExecuteBatchAsync(connectionsById, requests, cancellationToken);
        EnsureBatchResultCount(requests.Count, results.Count);

        var firstFailure = results.FirstOrDefault(result => !result.Passed);
        activity.SetOutcomeCode(firstFailure?.OutcomeCode ?? AssertionOutcomeCodes.Passed);
        activity.SetAttemptCount(results.Sum(result => result.AttemptCount));
        return results;
    }

    // islevi: Batch isteklerini kendi baglantilariyla sirayla calistirip oge-bazli sonuclari uretir.
    protected virtual async Task<List<RowAssertionResult>> ExecuteBatchAsync(
        IReadOnlyDictionary<Guid, DatabaseConnection> connectionsById,
        List<RowAssertionRequest> requests,
        CancellationToken cancellationToken)
    {
        var results = new List<RowAssertionResult>(requests.Count);
        foreach (var request in requests)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var connection = GetConnection(connectionsById, request.ConnectionId);
            results.Add(await AssertAsync(connection, request, cancellationToken));
        }

        return results;
    }

    // islevi: Batch sonucunun her istek icin tam bir oge tasidigini fail-closed dogrular.
    private static void EnsureBatchResultCount(int requestCount, int resultCount)
    {
        if (resultCount != requestCount)
        {
            throw new BusinessException(AssertionExceptionCodes.Validation.BatchResultCountMismatch);
        }
    }

    // islevi: Tum assertion uclarinin kullandigi tek domain cekirdegini senaryo adimlari sirasi ile calistirir.
    public virtual async Task<RowAssertionResult> AssertAsync(
        DatabaseConnection connection,
        RowAssertionRequest request,
        CancellationToken cancellationToken = default)
    {
        using var activity = DatabaseCheckerTelemetry.StartActivity(
            DatabaseCheckerTelemetryConstants.Activities.AssertRow,
            connection.Engine.Code,
            connection.DatabaseName);
        ValidateRequest(request);
        var settings = await _settingsResolver.ResolveAsync();
        var timing = ResolveTiming(request, settings);
        var structure = await _dataManager.ResolveAssertionStructureAsync(
            connection,
            request.SchemaName,
            request.TableName,
            cancellationToken);
        var catalogFailure = ValidateCatalogTarget(structure, request);
        if (catalogFailure is not null)
        {
            catalogFailure.Correlation = request.Correlation;
            SetTelemetryResult(activity, catalogFailure);
            return catalogFailure;
        }

        var retentionPolicy = await _retentionPolicyResolver.ResolveAsync();
        var result = await PollUntilFinalAsync(
            connection, request, structure!, settings, timing, retentionPolicy, cancellationToken);
        result.Correlation = request.Correlation;
        SetTelemetryResult(activity, result);
        return result;
    }

    // islevi: Assertion sonucunun kararli outcome ve deneme sayisini izinli span attribute'larina yazar.
    private static void SetTelemetryResult(
        DatabaseCheckerActivityScope activity,
        RowAssertionResult result)
    {
        activity.SetOutcomeCode(result.OutcomeCode);
        activity.SetAttemptCount(result.AttemptCount);
    }

    // islevi: Batch boyutunun tenant-aware ayar tavanini asmadigini dogrular.
    private async Task EnsureBatchSizeAsync(int count)
    {
        var settings = await _settingsResolver.ResolveAsync();
        if (count == 0 || count > settings.MaxBatchSize)
        {
            throw new BusinessException(
                count == 0
                    ? AssertionExceptionCodes.Validation.BatchRequired
                    : AssertionExceptionCodes.Validation.BatchTooLarge);
        }
    }

    // islevi: Batch'te erisilemeyen baglantiyi ABP not-found davranisina cevirir.
    private static DatabaseConnection GetConnection(
        IReadOnlyDictionary<Guid, DatabaseConnection> connections,
        Guid connectionId)
        => connections.GetValueOrDefault(connectionId)
           ?? throw new EntityNotFoundException(typeof(DatabaseConnection), connectionId);

    // islevi: Domain cagiranlari icin matcher/cardinality operandlarinin temel is kurallarini savunur.
    private static void ValidateRequest(RowAssertionRequest request)
    {
        if (!CardinalityKindCodes.IsDefined(request.Cardinality.KindCode))
        {
            throw new BusinessException(AssertionExceptionCodes.InvalidCardinalityKind);
        }

        if (request.Expectations.Any(item => !MatcherKindCodes.IsDefined(item.MatcherKindCode)))
        {
            throw new BusinessException(AssertionExceptionCodes.InvalidMatcherKind);
        }
    }

    // islevi: Istek timeout'ini ayar tavanina kirpar, poll araligini ayar tabanina yukseltir.
    private static (int TimeoutMs, int PollIntervalMs) ResolveTiming(
        RowAssertionRequest request,
        AssertionExecutionSettings settings)
    {
        var timeoutMs = Math.Clamp(request.TimeoutMs, 0, settings.MaxTimeoutMs);
        var requestedPollMs = request.PollIntervalMs > 0
            ? request.PollIntervalMs
            : settings.MinPollIntervalMs;
        return (timeoutMs, Math.Max(requestedPollMs, settings.MinPollIntervalMs));
    }

    // islevi: Tablo/kolon varligi ve anahtar tekillik garantisini ilk hedef sorgusundan once outcome'a cevirir.
    private RowAssertionResult? ValidateCatalogTarget(
        TableDataStructureModel? structure,
        RowAssertionRequest request)
    {
        if (structure is null)
        {
            return CreateResult(AssertionOutcomeCodes.TableNotFound, 0, 0);
        }

        if (HasMissingColumn(structure, request))
        {
            return CreateResult(AssertionOutcomeCodes.ColumnNotFound, 0, 0);
        }

        return IsUniqueKey(structure, request.KeyValues.Keys)
            ? null
            : CreateResult(AssertionOutcomeCodes.KeyNotUnique, 0, 0);
    }

    // islevi: Anahtar veya beklenti kolonlarindan herhangi birinin katalog yapisinda eksik olup olmadigini bildirir.
    private static bool HasMissingColumn(TableDataStructureModel structure, RowAssertionRequest request)
    {
        var columns = structure.ColumnNames.ToHashSet(StringComparer.OrdinalIgnoreCase);
        return request.KeyValues.Count == 0 ||
               request.KeyValues.Keys.Any(column => !columns.Contains(column)) ||
               request.Expectations.Any(item => !columns.Contains(item.ColumnName));
    }

    // islevi: Istek anahtar kolon kumesinin PK veya filtresiz unique katalog anahtariyla tam eslestigini bildirir.
    private static bool IsUniqueKey(TableDataStructureModel structure, IEnumerable<string> requestedColumns)
    {
        var requested = requestedColumns.ToHashSet(StringComparer.OrdinalIgnoreCase);
        return structure.UniqueKeyColumnSets.Any(key =>
            key.Count == requested.Count && key.All(requested.Contains));
    }

    // islevi: Assertion'i ilk basarida veya timeout sonrasi hedefli son outcome uretilene kadar yoklar.
    private async Task<RowAssertionResult> PollUntilFinalAsync(
        DatabaseConnection connection,
        RowAssertionRequest request,
        TableDataStructureModel structure,
        AssertionExecutionSettings settings,
        (int TimeoutMs, int PollIntervalMs) timing,
        ValueRetentionPolicy retentionPolicy,
        CancellationToken cancellationToken)
    {
        var startedAt = _clock.Now;
        var attempt = 0;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            attempt++;
            var observation = await ReadAttemptAsync(
                connection, request, structure, settings, cancellationToken);
            var result = EvaluateAttempt(request, structure, observation, attempt, settings, retentionPolicy);
            if (result.Passed || IsFinalAttempt(startedAt, timing.TimeoutMs, attempt))
            {
                return FinalizeTimedOutResult(result, timing.TimeoutMs);
            }

            await Task.Delay(timing.PollIntervalMs, cancellationToken);
        }
    }

    // islevi: Tek deneme icin count ve gerekiyorsa matcher/redaction satirlarini mevcut data manager'dan okur.
    private Task<RowAssertionObservation> ReadAttemptAsync(
        DatabaseConnection connection,
        RowAssertionRequest request,
        TableDataStructureModel structure,
        AssertionExecutionSettings settings,
        CancellationToken cancellationToken)
    {
        var includeRows = request.Expectations.Count > 0 || request.IncludeRowOnFailure;
        return _dataManager.ReadAssertionObservationAsync(
            connection,
            structure,
            request.KeyValues,
            settings.MaxRowsPerAssertion,
            includeRows,
            cancellationToken);
    }

    // islevi: Bir denemenin cardinality ve kolon matcher sonucunu response modeline cevirir.
    private RowAssertionResult EvaluateAttempt(
        RowAssertionRequest request,
        TableDataStructureModel structure,
        RowAssertionObservation observation,
        int attempt,
        AssertionExecutionSettings settings,
        ValueRetentionPolicy retentionPolicy)
    {
        if (!MatchesCardinality(request.Cardinality, observation.RowCount))
        {
            return BuildCardinalityFailure(request, observation, attempt, retentionPolicy);
        }

        var failures = EvaluateExpectations(request, structure, observation, settings, retentionPolicy);
        return failures.Count == 0
            ? CreateResult(AssertionOutcomeCodes.Passed, observation.RowCount, attempt)
            : BuildValueFailure(request, observation, attempt, failures, retentionPolicy);
    }

    // islevi: Cardinality kodunu tam sayi karsilastirmasina yonlendirir.
    private static bool MatchesCardinality(CardinalityExpectation expectation, long observedCount)
        => expectation.KindCode switch
        {
            CardinalityKindCodes.Exactly => observedCount == expectation.ExpectedCount,
            CardinalityKindCodes.AtLeast => observedCount >= expectation.ExpectedCount,
            CardinalityKindCodes.None => observedCount == 0,
            _ => false
        };

    // islevi: Ilk gozlenen satirdaki tum kolon beklentilerini bagimsiz degerlendirip failure listesi kurar.
    private List<FailedExpectation> EvaluateExpectations(
        RowAssertionRequest request,
        TableDataStructureModel structure,
        RowAssertionObservation observation,
        AssertionExecutionSettings settings,
        ValueRetentionPolicy retentionPolicy)
    {
        var row = observation.Rows.FirstOrDefault();
        if (row is null)
        {
            return new List<FailedExpectation>();
        }

        return request.Expectations
            .Select(expectation => EvaluateExpectation(expectation, row, structure, settings, retentionPolicy))
            .Where(failure => failure is not null)
            .Cast<FailedExpectation>()
            .ToList();
    }

    // islevi: Tek kolon matcher'i basarisizsa retention uygulanmis hedefli failure modeli uretir.
    private FailedExpectation? EvaluateExpectation(
        ColumnExpectation expectation,
        TableDataRowModel row,
        TableDataStructureModel structure,
        AssertionExecutionSettings settings,
        ValueRetentionPolicy retentionPolicy)
    {
        row.Values.TryGetValue(expectation.ColumnName, out var observed);
        var column = structure.Columns.First(item =>
            string.Equals(item.Name, expectation.ColumnName, StringComparison.OrdinalIgnoreCase));
        if (_matcher.Evaluate(expectation, observed, column.CanonicalDataTypeCode, column.NumericScale, settings.RegexTimeoutMs))
        {
            return null;
        }

        return new FailedExpectation
        {
            ColumnName = expectation.ColumnName,
            MatcherKindCode = expectation.MatcherKindCode,
            ExpectedValue = FormatExpectedValue(expectation),
            ObservedValue = _redactor.Redact(observed, retentionPolicy)
        };
    }

    // islevi: OneOf listesini veya tek beklenen degeri kucuk failure ozetine cevirir.
    private static string? FormatExpectedValue(ColumnExpectation expectation)
        => expectation.MatcherKindCode == MatcherKindCodes.OneOf
            ? string.Join(",", expectation.ExpectedValues)
            : expectation.ExpectedValue;

    // islevi: Cardinality uyusmazligini opsiyonel redaction'li satir ozetiyle kurar.
    private RowAssertionResult BuildCardinalityFailure(
        RowAssertionRequest request,
        RowAssertionObservation observation,
        int attempt,
        ValueRetentionPolicy retentionPolicy)
    {
        var outcome = observation.RowCount == 0
            ? AssertionOutcomeCodes.RowNotFound
            : AssertionOutcomeCodes.CardinalityMismatch;
        var result = CreateResult(outcome, observation.RowCount, attempt);
        result.RowSummary = BuildRowSummary(request, observation, retentionPolicy);
        return result;
    }

    // islevi: Deger uyusmazligini failure listesi ve opsiyonel redaction'li satir ozetiyle kurar.
    private RowAssertionResult BuildValueFailure(
        RowAssertionRequest request,
        RowAssertionObservation observation,
        int attempt,
        List<FailedExpectation> failures,
        ValueRetentionPolicy retentionPolicy)
    {
        var result = CreateResult(AssertionOutcomeCodes.ValueMismatch, observation.RowCount, attempt);
        result.FailedExpectations = failures;
        result.RowSummary = BuildRowSummary(request, observation, retentionPolicy);
        return result;
    }

    // islevi: Istek isterse ilk satirin tum degerlerine mevcut retention politikasini uygular.
    private Dictionary<string, string?>? BuildRowSummary(
        RowAssertionRequest request,
        RowAssertionObservation observation,
        ValueRetentionPolicy retentionPolicy)
    {
        if (!request.IncludeRowOnFailure || observation.Rows.Count == 0)
        {
            return null;
        }

        return observation.Rows[0].Values.ToDictionary(
            pair => pair.Key,
            pair => _redactor.Redact(pair.Value, retentionPolicy),
            StringComparer.OrdinalIgnoreCase);
    }

    // islevi: Pozitif timeout'ta en az iki deneme yaptiktan sonra ABP saatine gore beklemenin bitip bitmedigini bildirir.
    private bool IsFinalAttempt(DateTime startedAt, int timeoutMs, int attempt)
        => timeoutMs == 0 ||
           attempt > 1 && (_clock.Now - startedAt).TotalMilliseconds >= timeoutMs;

    // islevi: Timeout ile sonlanan bulunamamis satiri kararli TimedOut outcome'una cevirir.
    private static RowAssertionResult FinalizeTimedOutResult(RowAssertionResult result, int timeoutMs)
    {
        if (timeoutMs > 0 && result.OutcomeCode == AssertionOutcomeCodes.RowNotFound)
        {
            result.OutcomeCode = AssertionOutcomeCodes.TimedOut;
        }

        return result;
    }

    // islevi: Outcome, sayim, UTC epoch-millis ve deneme sayisini ortak response govdesinde kurar.
    private RowAssertionResult CreateResult(string outcomeCode, long observedCount, int attempt)
        => new()
        {
            OutcomeCode = outcomeCode,
            Passed = outcomeCode == AssertionOutcomeCodes.Passed,
            ObservedRowCount = observedCount,
            ObservedAtMs = new DateTimeOffset(_clock.Now.ToUniversalTime()).ToUnixTimeMilliseconds(),
            AttemptCount = attempt
        };
}
