using Ptn.ApiContractChecker.Constants.Conformance;
using Ptn.ApiContractChecker.Constants.Conformance.Lookups;
using Ptn.ApiContractChecker.Entities.Snapshots;
using Ptn.ApiContractChecker.Interface.Snapshots;
using Ptn.ApiContractChecker.Models.Conformance;
using Ptn.ApiContractChecker.Models.Snapshots;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Conformance;

// islevi: Response ve request alanlarini yalniz normalize ad ile tip uyumunda eslestirir.
// sistemdeki gorevi: Tek snapshot icinde aciklanabilir ve en fazla bes ODG onceki-operasyon onerisi uretir.
public class OperationBindingSuggester : ITransientDependency
{
    private readonly ISpecSchemaResolver _schemaResolver;
    private readonly OperationResolver _operationResolver;

    public OperationBindingSuggester(ISpecSchemaResolver schemaResolver, OperationResolver operationResolver)
    {
        _schemaResolver = schemaResolver;
        _operationResolver = operationResolver;
    }

    public async Task<OperationBindingResult> SuggestAsync(
        SpecSnapshot? snapshot,
        OperationSelectionRequest request)
    {
        if (snapshot?.SpecContent == null)
        {
            return Empty(ConformanceOutcomeCodes.SnapshotNotFound);
        }

        var model = await _schemaResolver.GetSnapshotAsync(snapshot.SpecContent);
        var target = _operationResolver.Resolve(model, request.OperationId, request.Method, request.Path);
        if (target == null)
        {
            return Empty(ConformanceOutcomeCodes.OperationNotResolved);
        }

        var result = new OperationBindingResult(
            ConformanceOutcomeCodes.Passed,
            BuildSuggestions(model.Operations, target));
        result.TrimToBudget();
        return result;
    }

    private static List<OperationBindingSuggestion> BuildSuggestions(
        IEnumerable<SpecOperationModel> operations,
        SpecOperationModel target)
    {
        return operations
            .Where(source => !ReferenceEquals(source, target))
            .Select(source => BuildSuggestion(source, target))
            .Where(suggestion => suggestion.Bindings.Count > 0)
            .OrderByDescending(suggestion => suggestion.Score)
            .ThenBy(suggestion => suggestion.SourcePath, StringComparer.Ordinal)
            .ThenBy(suggestion => suggestion.SourceMethod, StringComparer.Ordinal)
            .ToList();
    }

    private static OperationBindingSuggestion BuildSuggestion(
        SpecOperationModel source,
        SpecOperationModel target)
    {
        var targetFields = GetRequestFields(target);
        var bindings = GetResponseFields(source)
            .SelectMany(sourceField => targetFields
                .Where(targetField => CalculateScore(sourceField, targetField) > 0)
                .Select(targetField => new OperationFieldBinding(
                    sourceField.Pointer,
                    targetField.Pointer,
                    sourceField.Type,
                    CalculateScore(sourceField, targetField),
                    BuildExpression(source, sourceField, target, targetField))))
            .OrderByDescending(binding => binding.Score)
            .ThenBy(binding => binding.SourcePointer, StringComparer.Ordinal)
            .ThenBy(binding => binding.TargetPointer, StringComparer.Ordinal)
            .ToList();
        return new OperationBindingSuggestion(
            source.OperationId, source.Method, source.Path, bindings, bindings.Sum(binding => binding.Score));
    }

    private static List<Field> GetRequestFields(SpecOperationModel operation)
    {
        var fields = operation.Parameters.Select(parameter => new Field(
            parameter.Name,
            parameter.Type,
            BuildPointer(parameter.In, parameter.Name))).ToList();
        fields.AddRange(operation.RequestBodies
            .SelectMany(body => body.Schema?.Properties ?? [])
            .Select(property => new Field(
                property.Name,
                ResolveType(property),
                BuildPointer(ConformanceAuthoringConstants.BodyPointerSegment, property.Name))));
        return fields;
    }

    private static List<Field> GetResponseFields(SpecOperationModel operation)
    {
        return operation.Responses
            .Where(response => int.TryParse(response.StatusCode, out var status) && status is >= 200 and <= 299)
            .SelectMany(response => response.Schema?.Properties ?? [])
            .Select(property => new Field(
                property.Name,
                ResolveType(property),
                BuildPointer(ConformanceAuthoringConstants.BodyPointerSegment, property.Name)))
            .DistinctBy(field => string.Concat(field.Pointer, field.Type))
            .ToList();
    }

    private static int CalculateScore(Field source, Field target)
    {
        if (string.IsNullOrWhiteSpace(source.Type) ||
            !string.Equals(source.Type, target.Type, StringComparison.OrdinalIgnoreCase))
        {
            return 0;
        }

        var sourceName = NormalizeName(source.Name);
        var targetName = NormalizeName(target.Name);
        var nameScore = sourceName == targetName
            ? 3
            : sourceName == "id" && targetName.EndsWith("id", StringComparison.Ordinal) ||
              targetName == "id" && sourceName.EndsWith("id", StringComparison.Ordinal)
                ? 2
                : 0;
        return nameScore == 0 ? 0 : nameScore + 3;
    }

    private static string BuildExpression(
        SpecOperationModel source,
        Field sourceField,
        SpecOperationModel target,
        Field targetField)
    {
        return string.Concat(
            ResolveOperationReference(source),
            ConformanceAuthoringConstants.BindingMemberSeparator,
            sourceField.Name,
            ConformanceAuthoringConstants.BindingArrow,
            ResolveOperationReference(target),
            ConformanceAuthoringConstants.BindingMemberSeparator,
            targetField.Name);
    }

    private static string ResolveOperationReference(SpecOperationModel operation)
        => operation.OperationId ?? string.Concat(
            operation.Method,
            ConformanceAuthoringConstants.OperationReferenceSeparator,
            operation.Path);

    private static string NormalizeName(string name)
    {
        return new string(name.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
    }

    private static string? ResolveType(SpecSchemaPropertyModel property)
    {
        return property.Schema?.Type ?? property.Type;
    }

    private static string BuildPointer(string location, string name)
    {
        return string.Concat(
            ConformanceTextConstants.JsonPointerSeparator,
            location,
            ConformanceTextConstants.JsonPointerSeparator,
            name);
    }

    private static OperationBindingResult Empty(string outcome)
    {
        return new OperationBindingResult(outcome, new List<OperationBindingSuggestion>());
    }

    private sealed record Field(string Name, string? Type, string Pointer);
}
