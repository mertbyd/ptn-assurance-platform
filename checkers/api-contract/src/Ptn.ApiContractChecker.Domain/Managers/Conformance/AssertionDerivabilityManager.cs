using Ptn.ApiContractChecker.Constants.Conformance.Lookups;
using Ptn.ApiContractChecker.Entities.Snapshots;
using Ptn.ApiContractChecker.Interface.Snapshots;
using Ptn.ApiContractChecker.Models.Conformance;
using Ptn.ApiContractChecker.Models.Snapshots;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Conformance;

// islevi: Assertion JSON Pointer yollarinin snapshot response semasindan turetilebilirligini denetler.
// sistemdeki gorevi: G2 kapisinda implementation degerine bakmadan sozlesme disi ve kirilgan assertionlari ayirir.
public sealed class AssertionDerivabilityManager : ITransientDependency
{
    private readonly ISpecSchemaResolver _resolver;
    private readonly OperationResolver _operationResolver;

    public AssertionDerivabilityManager(ISpecSchemaResolver resolver, OperationResolver operationResolver)
    {
        _resolver = resolver;
        _operationResolver = operationResolver;
    }

    // islevi: Operasyon response semasini cozer ve her assertion yolunu kapali G2 koduna cevirir.
    public async Task<AssertionDerivabilityResult> ValidateAsync(
        SpecSnapshot? snapshot,
        AssertionDerivabilityRequest request)
    {
        var schema = await ResolveSchemaAsync(snapshot, request);
        var result = new AssertionDerivabilityResult();
        foreach (var path in request.AssertionPaths.Order(StringComparer.Ordinal))
        {
            result.Assertions.Add(new AssertionDerivabilityItem
            {
                JsonPointer = path,
                OutcomeCode = ClassifyPath(schema, path)
            });
        }

        result.TrimToBudget();
        return result;
    }

    // islevi: Hedef operasyonun belirtilen veya ilk kararli 2xx response semasini bulur.
    private async Task<SpecSchemaModel?> ResolveSchemaAsync(
        SpecSnapshot? snapshot,
        AssertionDerivabilityRequest request)
    {
        if (snapshot?.SpecContent is null)
        {
            return null;
        }

        var model = await _resolver.GetSnapshotAsync(snapshot.SpecContent);
        var operation = _operationResolver.Resolve(model, request.OperationId, request.Method, request.Path);
        return operation?.Responses
            .Where(response => MatchesResponse(response, request.StatusCode, request.MediaType))
            .OrderBy(response => response.StatusCode, StringComparer.Ordinal)
            .ThenBy(response => response.MediaType, StringComparer.Ordinal)
            .Select(response => response.Schema)
            .FirstOrDefault(schema => schema is not null);
    }

    // islevi: Assertion yolunu sema alanlarinda yuruyup opsiyonellik bilgisini biriktirir.
    private static string ClassifyPath(SpecSchemaModel? schema, string path)
    {
        if (schema is null || !path.StartsWith('/'))
        {
            return AssertionDerivabilityCodes.AssertionNotInContract;
        }

        var optional = false;
        foreach (var segment in path.Split('/', StringSplitOptions.RemoveEmptyEntries).Select(Unescape))
        {
            var property = schema.Properties.FirstOrDefault(candidate => candidate.Name == segment);
            if (property is null)
            {
                return AssertionDerivabilityCodes.AssertionNotInContract;
            }

            optional |= !property.Required || property.Nullable;
            schema = property.Schema ?? new SpecSchemaModel { Type = property.Type };
        }

        return optional
            ? AssertionDerivabilityCodes.DerivableButOptional
            : AssertionDerivabilityCodes.Derivable;
    }

    // islevi: Response secimini 2xx, opsiyonel status ve medya tipiyle sinirlar.
    private static bool MatchesResponse(SpecResponseModel response, string? statusCode, string? mediaType)
        => int.TryParse(response.StatusCode, out var status) && status is >= 200 and <= 299 &&
           (statusCode is null || response.StatusCode == statusCode) &&
           (mediaType is null || string.Equals(response.MediaType, mediaType, StringComparison.OrdinalIgnoreCase));

    // islevi: JSON Pointer kacislarini property adina geri cevirir.
    private static string Unescape(string segment) => segment.Replace("~1", "/").Replace("~0", "~");
}
