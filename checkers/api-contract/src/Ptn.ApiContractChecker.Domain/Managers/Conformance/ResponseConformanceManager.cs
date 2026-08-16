using NJsonSchema.Validation;
using Ptn.ApiContractChecker.Constants.Conformance;
using Ptn.ApiContractChecker.Constants.Conformance.Lookups;
using Ptn.ApiContractChecker.Entities.Snapshots;
using Ptn.ApiContractChecker.Interface.Snapshots;
using Ptn.ApiContractChecker.Models.Conformance;
using Ptn.ApiContractChecker.Models.Snapshots;
using Volo.Abp.Domain.Services;

namespace Ptn.ApiContractChecker.Managers.Conformance;

// islevi: Gozlenen HTTP yanitini snapshot operasyonunun yedi conformance kuralina karsi denetler.
// sistemdeki gorevi: Test Module'a tahminsiz, deger tasimayan ve butcelenmis assertion sonucu veren domain oracle'idir.
public class ResponseConformanceManager : DomainService
{
    private readonly OperationResolver _operationResolver;
    private readonly ISpecSchemaResolver _schemaResolver;
    private readonly ConformancePolicyResolver _policyResolver;
    private readonly ConformanceSettingsResolver _settingsResolver;

    public ResponseConformanceManager(
        OperationResolver operationResolver,
        ISpecSchemaResolver schemaResolver,
        ConformancePolicyResolver policyResolver,
        ConformanceSettingsResolver settingsResolver)
    {
        _operationResolver = operationResolver;
        _schemaResolver = schemaResolver;
        _policyResolver = policyResolver;
        _settingsResolver = settingsResolver;
    }

    public async Task<ResponseConformanceResult> AssertResponseAsync(
        SpecSnapshot? snapshot,
        ResponseConformanceRequest request)
    {
        var evaluation = await CreateEvaluationAsync(snapshot, request, isResponse: true);
        EvaluateNotAServerError(evaluation);
        EvaluateStatusCodeConformance(evaluation);
        EvaluateContentTypeConformance(evaluation);
        EvaluateHeadersConformance(evaluation);
        await EvaluateResponseSchemaConformanceAsync(evaluation);
        EvaluateAdditionalProperties(evaluation);
        EvaluateSecurityRequirement(evaluation);
        return await BuildResultAsync(evaluation);
    }

    // Request tarafi ayni yedi kural ve politika adimindan gecer; response'a ozel iki adim no-op'tur.
    public async Task<ResponseConformanceResult> AssertRequestAsync(
        SpecSnapshot? snapshot,
        RequestConformanceRequest request)
    {
        var evaluation = await CreateEvaluationAsync(snapshot, request, isResponse: false);
        EvaluateNotAServerError(evaluation);
        EvaluateStatusCodeConformance(evaluation);
        EvaluateContentTypeConformance(evaluation);
        EvaluateHeadersConformance(evaluation);
        await EvaluateResponseSchemaConformanceAsync(evaluation);
        EvaluateAdditionalProperties(evaluation);
        EvaluateSecurityRequirement(evaluation);
        return await BuildResultAsync(evaluation);
    }

    private async Task<Evaluation> CreateEvaluationAsync(
        SpecSnapshot? snapshot,
        IConformanceObservation request,
        bool isResponse)
    {
        if (snapshot?.SpecContent == null)
        {
            return new Evaluation(request, isResponse);
        }

        var model = await _schemaResolver.GetSnapshotAsync(snapshot.SpecContent);
        var operation = _operationResolver.Resolve(model, request.OperationId, request.Method, request.Path);
        return new Evaluation(request, isResponse, snapshot.SpecContent, operation);
    }

    private void EvaluateNotAServerError(Evaluation evaluation)
    {
        if (evaluation.IsResponse &&
            ((ResponseConformanceRequest)evaluation.Request).StatusCode >= 500)
        {
            AddViolation(evaluation, ConformanceRuleCodes.NotAServerError,
                ConformanceSchemaKeywords.StatusCode, ConformanceOutcomeCodes.ServerError);
        }
    }

    private void EvaluateStatusCodeConformance(Evaluation evaluation)
    {
        if (!evaluation.IsResponse || evaluation.Operation == null)
        {
            return;
        }

        var statusCode = ((ResponseConformanceRequest)evaluation.Request).StatusCode
            .ToString(System.Globalization.CultureInfo.InvariantCulture);
        evaluation.StatusResponses = evaluation.Operation.Responses.Where(response =>
            response.StatusCode == statusCode || response.StatusCode == ConformanceTextConstants.DefaultResponse).ToList();
        if (evaluation.StatusResponses.Count == 0)
        {
            AddViolation(evaluation, ConformanceRuleCodes.StatusCodeConformance,
                ConformanceSchemaKeywords.StatusCode, ConformanceOutcomeCodes.StatusCodeUndocumented);
        }
    }

    private void EvaluateContentTypeConformance(Evaluation evaluation)
    {
        if (evaluation.Operation == null)
        {
            return;
        }

        var mediaType = NormalizeMediaType(evaluation.Request.ContentType);
        var documented = evaluation.IsResponse
            ? SelectResponse(evaluation, mediaType)
            : SelectRequestBody(evaluation, mediaType);
        if (!documented)
        {
            AddViolation(evaluation, ConformanceRuleCodes.ContentTypeConformance,
                ConformanceSchemaKeywords.ContentType, ConformanceOutcomeCodes.MediaTypeUndocumented);
        }
    }

    private void EvaluateHeadersConformance(Evaluation evaluation)
    {
        if (evaluation.IsResponse)
        {
            EvaluateResponseHeaders(evaluation);
            return;
        }

        EvaluateRequestParameters(evaluation);
    }

    private void EvaluateResponseHeaders(Evaluation evaluation)
    {
        if (evaluation.Response == null)
        {
            return;
        }

        var observed = evaluation.Request.Headers.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var header in evaluation.Response.Headers.Where(header => header.Required && !observed.Contains(header.Name)))
        {
            AddViolation(evaluation, ConformanceRuleCodes.ResponseHeadersConformance,
                BuildHeaderPointer(header.Name), ConformanceSchemaKeywords.Required,
                ConformanceOutcomeCodes.RequiredHeaderMissing);
        }
    }

    private void EvaluateRequestParameters(Evaluation evaluation)
    {
        if (evaluation.Operation == null || evaluation.Request is not RequestConformanceRequest request)
        {
            return;
        }

        foreach (var parameter in evaluation.Operation.Parameters.Where(parameter => parameter.Required))
        {
            if (!HasObservedParameter(parameter, request))
            {
                AddViolation(evaluation, ConformanceRuleCodes.ResponseHeadersConformance,
                    BuildParameterPointer(parameter), ConformanceSchemaKeywords.Required,
                    ConformanceOutcomeCodes.ResponseSchemaViolation);
            }
        }
    }

    private async Task EvaluateResponseSchemaConformanceAsync(Evaluation evaluation)
    {
        if (evaluation.Content == null || evaluation.Operation == null)
        {
            return;
        }

        if (!evaluation.Request.Body.HasValue)
        {
            AddMissingBodyViolation(evaluation);
            return;
        }

        var schema = await ResolveSchemaAsync(evaluation);
        if (schema == null)
        {
            return;
        }

        var settings = new JsonSchemaValidatorSettings { PropertyStringComparer = StringComparer.Ordinal };
        evaluation.SchemaErrors = Flatten(schema.SchemaNode.Validate(
            evaluation.Request.Body.Value.GetRawText(), schema.SchemaType, settings)).ToList();
        AddSchemaErrors(evaluation, includeAdditionalProperties: false);
    }

    private void EvaluateAdditionalProperties(Evaluation evaluation)
    {
        AddSchemaErrors(evaluation, includeAdditionalProperties: true);
    }

    private static void EvaluateSecurityRequirement(Evaluation evaluation)
    {
        // Response gozlemi request credential'ini kanitlamaz; tahmin yapmamak bu kuralin fail-closed davranisidir.
    }

    private static bool SelectResponse(Evaluation evaluation, string mediaType)
    {
        if (evaluation.StatusResponses.Count == 0)
        {
            return false;
        }

        evaluation.Response = evaluation.StatusResponses.FirstOrDefault(response =>
            string.Equals(response.MediaType, mediaType, StringComparison.OrdinalIgnoreCase));
        return evaluation.Response != null;
    }

    private static bool SelectRequestBody(Evaluation evaluation, string mediaType)
    {
        evaluation.RequestBody = evaluation.Operation!.RequestBodies.FirstOrDefault(body =>
            string.Equals(body.MediaType, mediaType, StringComparison.OrdinalIgnoreCase));
        return evaluation.RequestBody != null || evaluation.Operation.RequestBodies.Count == 0;
    }

    private async Task<ResolvedSpecSchemaModel?> ResolveSchemaAsync(Evaluation evaluation)
    {
        if (!evaluation.IsResponse)
        {
            return evaluation.RequestBody == null
                ? null
                : await _schemaResolver.ResolveRequestAsync(
                    evaluation.Content!, evaluation.Operation!, evaluation.RequestBody.MediaType);
        }

        return evaluation.Response == null
            ? null
            : await _schemaResolver.ResolveAsync(
                evaluation.Content!, evaluation.Operation!, evaluation.Response.StatusCode, evaluation.Response.MediaType);
    }

    private void AddMissingBodyViolation(Evaluation evaluation)
    {
        if (!evaluation.IsResponse &&
            evaluation.RequestBody?.Required == true &&
            evaluation.Candidates.Count == 0)
        {
            AddViolation(evaluation, ConformanceRuleCodes.ResponseSchemaConformance,
                ConformanceSchemaKeywords.Required, ConformanceOutcomeCodes.ResponseSchemaViolation);
        }
    }

    private static bool HasObservedParameter(
        SpecParameterModel parameter,
        RequestConformanceRequest request)
    {
        return parameter.In switch
        {
            ParameterLocationCodes.Query => request.Query.ContainsKey(parameter.Name),
            ParameterLocationCodes.Header => request.Headers.Keys.Contains(parameter.Name, StringComparer.OrdinalIgnoreCase),
            ParameterLocationCodes.Path => true,
            _ => true
        };
    }

    private static string BuildParameterPointer(SpecParameterModel parameter)
    {
        return string.Concat(
            ConformanceTextConstants.JsonPointerSeparator,
            EscapePointerSegment(parameter.In),
            ConformanceTextConstants.JsonPointerSeparator,
            EscapePointerSegment(parameter.Name));
    }

    private void AddSchemaErrors(Evaluation evaluation, bool includeAdditionalProperties)
    {
        foreach (var error in evaluation.SchemaErrors.Where(error =>
                     IsAdditionalPropertyError(error.Kind) == includeAdditionalProperties))
        {
            var rule = includeAdditionalProperties
                ? ConformanceRuleCodes.AdditionalProperties
                : ConformanceRuleCodes.ResponseSchemaConformance;
            var outcome = includeAdditionalProperties
                ? ConformanceOutcomeCodes.UndocumentedProperty
                : ConformanceOutcomeCodes.ResponseSchemaViolation;
            AddViolation(evaluation, rule, BuildPointer(error), ResolveKeyword(error.Kind), outcome);
        }
    }

    private void AddViolation(Evaluation evaluation, string rule, string keyword, string outcome)
    {
        AddViolation(evaluation, rule, ConformanceTextConstants.JsonPointerRoot, keyword, outcome);
    }

    private void AddViolation(Evaluation evaluation, string rule, string pointer, string keyword, string outcome)
    {
        var level = _policyResolver.Resolve(evaluation.Request.ProfileCode, rule);
        evaluation.Candidates.Add(new ConformanceViolation(rule, pointer, keyword, level, outcome));
    }

    private async Task<ResponseConformanceResult> BuildResultAsync(Evaluation evaluation)
    {
        if (evaluation.Content == null)
        {
            return new ResponseConformanceResult(
                ConformanceOutcomeCodes.SnapshotNotFound,
                new List<ConformanceViolation>(),
                evaluation.Request.Correlation);
        }

        if (evaluation.Operation == null)
        {
            return new ResponseConformanceResult(
                ConformanceOutcomeCodes.OperationNotResolved,
                new List<ConformanceViolation>(),
                evaluation.Request.Correlation);
        }

        var included = evaluation.Candidates.Where(item => item.LevelCode != ConformanceLevelCodes.Ignore).ToList();
        var outcome = ResolveOutcome(evaluation.Candidates, included);
        var result = new ResponseConformanceResult(outcome, included, evaluation.Request.Correlation);
        var limits = await _settingsResolver.ResolveAsync();
        result.TrimToBudget(limits.MaxViolations, limits.MaxResponseBytes);
        return result;
    }

    private static string ResolveOutcome(
        IReadOnlyCollection<ConformanceViolation> candidates,
        IReadOnlyCollection<ConformanceViolation> included)
    {
        var failure = included.FirstOrDefault(item => item.LevelCode == ConformanceLevelCodes.Fail);
        if (failure != null)
        {
            return failure.OutcomeCode;
        }

        return candidates.Count > 0 && included.Count == 0
            ? ConformanceOutcomeCodes.PolicySuppressed
            : ConformanceOutcomeCodes.Passed;
    }

    private static string NormalizeMediaType(string? contentType)
    {
        return (contentType ?? string.Empty)
            .Split(ConformanceTextConstants.ContentTypeParameterSeparator, 2)[0]
            .Trim()
            .ToLowerInvariant();
    }

    private static string BuildHeaderPointer(string headerName)
    {
        return string.Concat(
            ConformanceTextConstants.JsonPointerSeparator,
            ConformanceTextConstants.HeadersPointerSegment,
            ConformanceTextConstants.JsonPointerSeparator,
            EscapePointerSegment(headerName));
    }

    private static string BuildPointer(ValidationError error)
    {
        var path = error.Path ?? string.Empty;
        if (path.StartsWith(ConformanceTextConstants.JsonPointerFragmentPrefix, StringComparison.Ordinal))
        {
            path = path[1..];
        }
        else
        {
            path = path.TrimStart(ConformanceTextConstants.JsonPathRoot.ToCharArray())
                .Replace(ConformanceTextConstants.JsonPathPropertySeparator,
                    ConformanceTextConstants.JsonPointerSeparator, StringComparison.Ordinal);
        }

        return path.Length == 0 ? ConformanceTextConstants.JsonPointerRoot :
            path.StartsWith(ConformanceTextConstants.JsonPointerSeparator, StringComparison.Ordinal)
                ? path
                : string.Concat(ConformanceTextConstants.JsonPointerSeparator, EscapePointerSegment(path));
    }

    private static string EscapePointerSegment(string value)
    {
        return value
            .Replace(ConformanceTextConstants.JsonPointerTilde,
                ConformanceTextConstants.JsonPointerEscapedTilde, StringComparison.Ordinal)
            .Replace(ConformanceTextConstants.JsonPointerSeparator,
                ConformanceTextConstants.JsonPointerEscapedSlash, StringComparison.Ordinal);
    }

    private static IEnumerable<ValidationError> Flatten(IEnumerable<ValidationError> errors)
    {
        foreach (var error in errors)
        {
            var children = GetChildren(error).ToList();
            if (children.Count == 0)
            {
                yield return error;
                continue;
            }

            foreach (var child in Flatten(children))
            {
                yield return child;
            }
        }
    }

    private static IEnumerable<ValidationError> GetChildren(ValidationError error)
    {
        if (error is ChildSchemaValidationError child)
        {
            return child.Errors.Values.SelectMany(items => items);
        }

        return error is MultiTypeValidationError multi
            ? multi.Errors.Values.SelectMany(items => items)
            : Enumerable.Empty<ValidationError>();
    }

    private static bool IsAdditionalPropertyError(ValidationErrorKind kind)
    {
        return kind is ValidationErrorKind.NoAdditionalPropertiesAllowed or
            ValidationErrorKind.AdditionalPropertiesNotValid;
    }

    private static string ResolveKeyword(ValidationErrorKind kind)
    {
        return kind switch
        {
            ValidationErrorKind.PropertyRequired => ConformanceSchemaKeywords.Required,
            ValidationErrorKind.PatternMismatch => ConformanceSchemaKeywords.Pattern,
            ValidationErrorKind.StringTooShort => ConformanceSchemaKeywords.MinLength,
            ValidationErrorKind.StringTooLong => ConformanceSchemaKeywords.MaxLength,
            ValidationErrorKind.NumberTooSmall => ConformanceSchemaKeywords.Minimum,
            ValidationErrorKind.NumberTooBig => ConformanceSchemaKeywords.Maximum,
            ValidationErrorKind.TooFewItems => ConformanceSchemaKeywords.MinItems,
            ValidationErrorKind.TooManyItems => ConformanceSchemaKeywords.MaxItems,
            ValidationErrorKind.ItemsNotUnique => ConformanceSchemaKeywords.UniqueItems,
            ValidationErrorKind.NotInEnumeration => ConformanceSchemaKeywords.Enum,
            ValidationErrorKind.NotAnyOf => ConformanceSchemaKeywords.AnyOf,
            ValidationErrorKind.NotAllOf => ConformanceSchemaKeywords.AllOf,
            ValidationErrorKind.NotOneOf => ConformanceSchemaKeywords.OneOf,
            ValidationErrorKind.ExcludedSchemaValidates => ConformanceSchemaKeywords.Not,
            ValidationErrorKind.NoAdditionalPropertiesAllowed => ConformanceSchemaKeywords.AdditionalProperties,
            ValidationErrorKind.AdditionalPropertiesNotValid => ConformanceSchemaKeywords.AdditionalProperties,
            _ => ConformanceSchemaKeywords.Type
        };
    }

    private sealed class Evaluation
    {
        public IConformanceObservation Request { get; }
        public bool IsResponse { get; }
        public SpecContent? Content { get; }
        public SpecOperationModel? Operation { get; }
        public List<SpecResponseModel> StatusResponses { get; set; } = new();
        public SpecResponseModel? Response { get; set; }
        public SpecRequestBodyModel? RequestBody { get; set; }
        public List<ValidationError> SchemaErrors { get; set; } = new();
        public List<ConformanceViolation> Candidates { get; } = new();

        public Evaluation(
            IConformanceObservation request,
            bool isResponse,
            SpecContent? content = null,
            SpecOperationModel? operation = null)
        {
            Request = request;
            IsResponse = isResponse;
            Content = content;
            Operation = operation;
        }
    }
}
