using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Ptn.ApiContractChecker.Constants.Conformance.Lookups;
using Ptn.ApiContractChecker.Dtos.Conformance;
using Ptn.ApiContractChecker.Services.Conformance;
using Ptn.TestModule.Constants.Bridge.Vocabulary;
using Ptn.TestModule.ExceptionCodes.Bridge;
using Ptn.TestModule.Interface.Bridge;
using Ptn.TestModule.Models.Bridge;
using Volo.Abp;

namespace Ptn.TestModule.Adapters;

// islevi: API checker yazarlik ve uygunluk DTO'larini kopru modellerine ve tek outcome sozlugune cevirir.
// sistemdeki gorevi: API checker casing ve transport tiplerinin domain veya ajan yuzeyine sizmasini engeller.
public class ApiOracleAdapter : IApiOraclePort
{
    private readonly IResponseConformanceAppService _appService;

    // API checker public AppService'ini yalniz anti-corruption adapter'ina baglar.
    public ApiOracleAdapter(IResponseConformanceAppService appService)
    {
        _appService = appService;
    }

    // Operasyon sorgusunu checker DTO'suna cevirip normalize edilmis baglama sonucu dondurur.
    public async Task<PtnOperationBinding> SuggestOperationBindingsAsync(
        PtnOperationQuery query,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = await _appService.SuggestOperationBindingsAsync(MapQuery(query));
        return new PtnOperationBinding
        {
            OutcomeCode = NormalizeOutcome(result.OutcomeCode),
            Suggestions = result.Suggestions.Select(MapSuggestion).ToList()
        };
    }

    // Operasyon sorgusunu checker DTO'suna cevirip JSON parcalariyla request ornegi dondurur.
    public async Task<PtnRequestExample> BuildRequestExampleAsync(
        PtnOperationQuery query,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = await _appService.BuildRequestExampleAsync(MapQuery(query));
        return new PtnRequestExample
        {
            OutcomeCode = NormalizeOutcome(result.OutcomeCode),
            ValuesArePlaceholders = result.ValuesArePlaceholders,
            IsComplete = result.IsComplete,
            ContentType = result.ContentType,
            PathParameters = MapJsonValues(result.PathParameters),
            Query = MapJsonValues(result.Query),
            Headers = MapJsonValues(result.Headers),
            BodyJson = result.Body?.ToJsonString()
        };
    }

    // Turetilebilirlik istegini checker DTO'suna cevirip pointer outcome'larini normalize eder.
    public async Task<PtnDerivabilityResult> ValidateScenarioAssertionsAsync(
        PtnDerivabilityRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = await _appService.ValidateScenarioAssertionsAsync(new AssertionDerivabilityDto
        {
            SnapshotId = request.SnapshotId,
            OperationId = request.OperationId,
            Method = request.Method,
            Path = request.Path,
            StatusCode = request.StatusCode,
            MediaType = request.MediaType,
            AssertionPaths = request.AssertionPaths
        });
        return MapDerivability(result);
    }

    // Gozlenen response'u checker DTO'suna cevirip uygunluk sonucunu deger sizdirmadan normalize eder.
    public async Task<PtnConformanceResult> AssertResponseAsync(
        PtnResponseObservation observation,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = await _appService.AssertResponseAsync(new ResponseConformanceDto
        {
            SnapshotId = observation.SnapshotId,
            OperationId = observation.OperationId,
            Method = observation.Method,
            Path = observation.Path,
            StatusCode = observation.StatusCode,
            ContentType = observation.ContentType,
            Headers = observation.Headers,
            Body = ParseBody(observation.BodyJson)
        });
        return MapConformance(result);
    }

    // Domain operasyon adresini API checker secim DTO'suna cevirir.
    private static OperationSelectionDto MapQuery(PtnOperationQuery query)
    {
        return new OperationSelectionDto
        {
            SnapshotId = query.SnapshotId,
            OperationId = query.OperationId,
            Method = query.Method,
            Path = query.Path
        };
    }

    // Checker operasyon onerisi ve alan baglarini davranissiz kopru modeline cevirir.
    private static PtnOperationSuggestion MapSuggestion(
        OperationBindingSuggestionDto suggestion)
    {
        return new PtnOperationSuggestion
        {
            SourceOperationId = suggestion.SourceOperationId,
            SourceMethod = suggestion.SourceMethod,
            SourcePath = suggestion.SourcePath,
            Score = suggestion.Score,
            Bindings = suggestion.Bindings.Select(binding => new PtnFieldBinding
            {
                SourcePointer = binding.SourcePointer,
                TargetPointer = binding.TargetPointer,
                TypeCode = binding.Type,
                Score = binding.Score,
                Expression = binding.Expression
            }).ToList()
        };
    }

    // JsonNode sozluklerini provider tipini sizdirmeyen JSON metin degerlerine cevirir.
    private static Dictionary<string, string?> MapJsonValues(Dictionary<string, JsonNode?> values)
    {
        return values.ToDictionary(
            pair => pair.Key,
            pair => pair.Value?.ToJsonString(),
            StringComparer.Ordinal);
    }

    // Checker turetilebilirlik sonucunu normalize edilmis pointer listesine cevirir.
    private static PtnDerivabilityResult MapDerivability(AssertionDerivabilityResultDto result)
    {
        return new PtnDerivabilityResult
        {
            IsTruncated = result.IsTruncated,
            Assertions = result.Assertions.Select(item => new PtnDerivabilityItem
            {
                JsonPointer = item.JsonPointer,
                OutcomeCode = NormalizeOutcome(item.OutcomeCode)
            }).ToList()
        };
    }

    // Checker uygunluk sonucunu normalize edilmis outcome ve deger icermeyen ihlallere cevirir.
    private static PtnConformanceResult MapConformance(ConformanceResultDto result)
    {
        return new PtnConformanceResult
        {
            OutcomeCode = NormalizeOutcome(result.OutcomeCode),
            Violations = result.Violations.Select(item => new PtnConformanceViolation
            {
                RuleCode = item.RuleCode,
                JsonPointer = item.JsonPointer,
                Keyword = item.Keyword
            }).ToList()
        };
    }

    // JSON response govdesini checker DTO'sunun JsonElement tipine guvenli bicimde cevirir.
    private static JsonElement? ParseBody(string? bodyJson)
    {
        if (string.IsNullOrWhiteSpace(bodyJson))
        {
            return null;
        }

        using var document = JsonDocument.Parse(bodyJson);
        return document.RootElement.Clone();
    }

    // Checker outcome kodunu tek kopru sozlugune cevirir; bilinmeyen drift'i hata yapar.
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
            [ConformanceOutcomeCodes.Passed] = PtnOutcomeCodes.Passed,
            [ConformanceOutcomeCodes.StatusCodeUndocumented] = PtnOutcomeCodes.StatusCodeUndocumented,
            [ConformanceOutcomeCodes.MediaTypeUndocumented] = PtnOutcomeCodes.MediaTypeUndocumented,
            [ConformanceOutcomeCodes.ResponseSchemaViolation] = PtnOutcomeCodes.ResponseSchemaViolation,
            [ConformanceOutcomeCodes.RequiredHeaderMissing] = PtnOutcomeCodes.RequiredHeaderMissing,
            [ConformanceOutcomeCodes.UndocumentedProperty] = PtnOutcomeCodes.UndocumentedProperty,
            [ConformanceOutcomeCodes.ServerError] = PtnOutcomeCodes.ServerError,
            [ConformanceOutcomeCodes.OperationNotResolved] = PtnOutcomeCodes.OperationNotResolved,
            [ConformanceOutcomeCodes.SnapshotNotFound] = PtnOutcomeCodes.SnapshotNotFound,
            [ConformanceOutcomeCodes.PolicySuppressed] = PtnOutcomeCodes.PolicySuppressed,
            [ConformanceOutcomeCodes.SchemaNotResolved] = PtnOutcomeCodes.SchemaNotResolved,
            [AssertionDerivabilityCodes.Derivable] = PtnOutcomeCodes.Derivable,
            [AssertionDerivabilityCodes.AssertionNotInContract] = PtnOutcomeCodes.AssertionNotInContract,
            [AssertionDerivabilityCodes.DerivableButOptional] = PtnOutcomeCodes.DerivableButOptional
        };
}
