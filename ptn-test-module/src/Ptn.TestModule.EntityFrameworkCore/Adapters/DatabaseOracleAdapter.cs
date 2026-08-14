using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ptn.DatabaseChecker.Constants.Comparison.Assertions;
using Ptn.DatabaseChecker.Dtos.Assertions;
using Ptn.DatabaseChecker.Services.Assertions;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Constants.Bridge.Vocabulary;
using Ptn.TestModule.ExceptionCodes.Bridge;
using Ptn.TestModule.Interface.Bridge;
using Ptn.TestModule.Models.Bridge;
using Volo.Abp;

namespace Ptn.TestModule.Adapters;

// islevi: Database checker assertion DTO'larini kopru modellerine, tek outcome casing'ine ve redaksiyonlu ozete cevirir.
// sistemdeki gorevi: Database checker transport tipleri ile ham satir degerlerinin domain ve ajan yuzeyine sizmasini engeller.
public class DatabaseOracleAdapter : IDatabaseOraclePort
{
    private readonly IDatabaseAssertionAppService _appService;

    // Database checker public assertion AppService'ini yalniz anti-corruption adapter'ina baglar.
    public DatabaseOracleAdapter(IDatabaseAssertionAppService appService)
    {
        _appService = appService;
    }

    // Satir assertion istegini checker DTO'suna cevirip normalize edilmis sonucu dondurur.
    public async Task<PtnAssertionResult> AssertRowAsync(
        PtnAssertionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _appService.AssertRowAsync(MapRequest(request), cancellationToken);
        return MapResult(result);
    }

    // Count assertion istegini checker DTO'suna cevirip normalize edilmis sonucu dondurur.
    public async Task<PtnAssertionResult> AssertCountAsync(
        PtnAssertionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _appService.AssertCountAsync(MapRequest(request), cancellationToken);
        return MapResult(result);
    }

    // Absence assertion istegini checker DTO'suna cevirip normalize edilmis sonucu dondurur.
    public async Task<PtnAssertionResult> AssertAbsentAsync(
        PtnAssertionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _appService.AssertAbsentAsync(MapRequest(request), cancellationToken);
        return MapResult(result);
    }

    // Bir assertion listesini tek checker cagrisi icin DTO listesine cevirip sonuclari sirayla normalize eder.
    public async Task<IReadOnlyList<PtnAssertionResult>> AssertBatchAsync(
        IReadOnlyList<PtnAssertionRequest> requests,
        CancellationToken cancellationToken)
    {
        var inputs = requests.Select(MapRequest).ToList();
        var results = await _appService.AssertBatchAsync(inputs, cancellationToken);
        return results.Select(MapResult).ToList();
    }

    // Checker'da projeksiyon ucu bulunmadigi surece kanit yoklugunu Unavailable olarak bildirir.
    public Task<PtnProjectionResult> ProjectAsync(
        PtnProjectionRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new PtnProjectionResult
        {
            StateCode = PtnEvidenceStateCodes.Unavailable
        });
    }

    // Domain assertion modelini secret veya serbest SQL icermeyen checker DTO'suna cevirir.
    private static RowAssertionRequestDto MapRequest(PtnAssertionRequest request)
    {
        return new RowAssertionRequestDto
        {
            ConnectionId = request.ConnectionId,
            SchemaName = request.DbSchemaName,
            TableName = request.TableName,
            KeyValues = request.KeyValues,
            Expectations = request.Expectations.Select(MapExpectation).ToList(),
            Cardinality = new CardinalityExpectationDto
            {
                KindCode = request.CardinalityKindCode,
                ExpectedCount = request.ExpectedCount
            },
            TimeoutMs = request.TimeoutMs,
            PollIntervalMs = request.PollIntervalMs,
            IncludeRowOnFailure = true
        };
    }

    // Tek domain kolon beklentisini checker matcher DTO'suna cevirir.
    private static ColumnExpectationDto MapExpectation(PtnAssertionRequest.PtnColumnExpectation expectation)
    {
        return new ColumnExpectationDto
        {
            ColumnName = expectation.ColumnName,
            MatcherKindCode = expectation.MatcherKindCode,
            ExpectedValue = expectation.ExpectedValue,
            ExpectedValues = expectation.ExpectedValues,
            Tolerance = expectation.Tolerance
        };
    }

    // Checker assertion sonucunu normalize eder ve tum deger alanlarini yeniden redakte eder.
    private static PtnAssertionResult MapResult(RowAssertionResultDto result)
    {
        return new PtnAssertionResult
        {
            OutcomeCode = NormalizeOutcome(result.OutcomeCode),
            Passed = result.Passed,
            ObservedRowCount = result.ObservedRowCount,
            ObservedAtMs = result.ObservedAtMs,
            AttemptCount = result.AttemptCount,
            FailedExpectations = result.FailedExpectations.Select(MapFailure).ToList(),
            RowSummary = RedactRow(result.RowSummary)
        };
    }

    // Basarisiz beklentinin kolon ve matcher adresini korurken degerlerini redakte eder.
    private static PtnAssertionResult.PtnFailedExpectation MapFailure(FailedExpectationDto failure)
    {
        return new PtnAssertionResult.PtnFailedExpectation
        {
            ColumnName = failure.ColumnName,
            MatcherKindCode = failure.MatcherKindCode,
            ExpectedValue = Redact(failure.ExpectedValue),
            ObservedValue = Redact(failure.ObservedValue)
        };
    }

    // Satir ozetinde yalniz kolon adlarini koruyup butun degerleri redaksiyon belirtecine cevirir.
    private static Dictionary<string, string?>? RedactRow(Dictionary<string, string?>? row)
    {
        return row?.ToDictionary(
            pair => pair.Key,
            pair => Redact(pair.Value),
            StringComparer.OrdinalIgnoreCase);
    }

    // Bos degeri bos tutar, mevcut her degeri kararli redaksiyon belirtecine cevirir.
    private static string? Redact(string? value)
    {
        return value is null ? null : PtnRedactionCodes.Redacted;
    }

    // Checker outcome kodunu tek kopru casing'ine cevirir; bilinmeyen drift'i hata yapar.
    private static string NormalizeOutcome(string outcomeCode)
    {
        if (OutcomeMap.TryGetValue(outcomeCode, out var normalized))
        {
            return normalized;
        }

        throw new BusinessException(TestModuleBridgeErrorCodes.CheckerCallFailed)
            .WithData(nameof(outcomeCode), outcomeCode);
    }

    private static readonly IReadOnlyDictionary<string, string> OutcomeMap =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [AssertionOutcomeCodes.Passed] = PtnOutcomeCodes.Passed,
            [AssertionOutcomeCodes.RowNotFound] = PtnOutcomeCodes.RowNotFound,
            [AssertionOutcomeCodes.ValueMismatch] = PtnOutcomeCodes.ValueMismatch,
            [AssertionOutcomeCodes.CardinalityMismatch] = PtnOutcomeCodes.CardinalityMismatch,
            [AssertionOutcomeCodes.TimedOut] = PtnOutcomeCodes.TimedOut,
            [AssertionOutcomeCodes.KeyNotUnique] = PtnOutcomeCodes.KeyNotUnique,
            [AssertionOutcomeCodes.TableNotFound] = PtnOutcomeCodes.TableNotFound,
            [AssertionOutcomeCodes.ColumnNotFound] = PtnOutcomeCodes.ColumnNotFound
        };
}
