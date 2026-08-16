using System.IO.Enumeration;
using Ptn.ApiContractChecker.Constants.Runs;
using Ptn.ApiContractChecker.Constants.Snapshots;
using Ptn.ApiContractChecker.ExceptionCodes.Runs;
using Ptn.ApiContractChecker.Models.Runs;
using Ptn.ApiContractChecker.Models.Snapshots;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Comparison;

// islevi: Request-scoped include/exclude kurallarini normalize edilmis OpenAPI fotografina uygular.
// sistemdeki gorevi: Kapsami tabloya yazmadan iki snapshot'a ayni deterministik filtreyi uygular; exclude her zaman include'dan once kazanir.
public class SpecComparisonScopeManager : ITransientDependency
{
    // Normalize edilmis snapshot'i operasyon, sema ve bagli dokumantasyon kapsamiyla yeni bir modele indirger.
    public SpecSnapshotModel Apply(
        SpecSnapshotModel snapshot,
        IReadOnlyCollection<ContractCheckScopeRuleModel> rules,
        bool ignoreInternal)
    {
        var operations = snapshot.Operations
            .Where(operation => IsOperationIncluded(operation, rules, ignoreInternal))
            .ToList();
        var schemas = snapshot.Schemas
            .Where(schema => IsSchemaIncluded(schema, rules, ignoreInternal))
            .ToList();
        var operationTargets = operations
            .Select(BuildOperationTarget)
            .ToHashSet(StringComparer.Ordinal);
        var schemaTargets = schemas
            .Select(schema => schema.Name)
            .ToHashSet(StringComparer.Ordinal);

        return new SpecSnapshotModel
        {
            Operations = operations,
            Schemas = schemas,
            Documentation = snapshot.Documentation
                .Where(item => IsDocumentationIncluded(item, operationTargets, schemaTargets))
                .ToList()
        };
    }

    // Exclusion eslesmesini once uygular; operation include kurali varsa en az biriyle eslesmeyi zorunlu kilar.
    private static bool IsOperationIncluded(
        SpecOperationModel operation,
        IReadOnlyCollection<ContractCheckScopeRuleModel> rules,
        bool ignoreInternal)
    {
        if (ignoreInternal && operation.IsInternal)
        {
            return false;
        }

        var operationRules = rules.Where(rule => rule.TargetCode != ContractCheckScopeCodes.Targets.Schema).ToList();
        if (operationRules.Any(rule => IsExclude(rule) && MatchesOperation(rule, operation)))
        {
            return false;
        }

        var includeRules = operationRules.Where(IsInclude).ToList();
        return includeRules.Count == 0 || includeRules.Any(rule => MatchesOperation(rule, operation));
    }

    // Exclusion eslesmesini once uygular; schema include kurali varsa en az biriyle eslesmeyi zorunlu kilar.
    private static bool IsSchemaIncluded(
        SpecSchemaModel schema,
        IReadOnlyCollection<ContractCheckScopeRuleModel> rules,
        bool ignoreInternal)
    {
        if (ignoreInternal && schema.IsInternal)
        {
            return false;
        }

        var schemaRules = rules.Where(rule => rule.TargetCode == ContractCheckScopeCodes.Targets.Schema).ToList();
        if (schemaRules.Any(rule => IsExclude(rule) && Matches(rule.Pattern, schema.Name)))
        {
            return false;
        }

        var includeRules = schemaRules.Where(IsInclude).ToList();
        return includeRules.Count == 0 || includeRules.Any(rule => Matches(rule.Pattern, schema.Name));
    }

    // Bir operasyonu kural hedefinin kendi kararli kimligine gore eslestirir.
    private static bool MatchesOperation(ContractCheckScopeRuleModel rule, SpecOperationModel operation)
    {
        return rule.TargetCode switch
        {
            ContractCheckScopeCodes.Targets.Path => Matches(rule.Pattern, operation.Path),
            ContractCheckScopeCodes.Targets.Tag => operation.Tags.Any(tag => Matches(rule.Pattern, tag)),
            ContractCheckScopeCodes.Targets.OperationId =>
                operation.OperationId != null && Matches(rule.Pattern, operation.OperationId),
            _ => throw new BusinessException(ContractCheckRunExceptionCodes.InvalidScopeRule)
        };
    }

    // Kural turunun include oldugunu dogrular; bilinmeyen tur runtime job payload'inda sessizce kabul edilmez.
    private static bool IsInclude(ContractCheckScopeRuleModel rule)
    {
        EnsureKnownKind(rule.KindCode);
        return rule.KindCode == ContractCheckScopeCodes.Kinds.Include;
    }

    // Kural turunun exclude oldugunu dogrular; bilinmeyen tur runtime job payload'inda sessizce kabul edilmez.
    private static bool IsExclude(ContractCheckScopeRuleModel rule)
    {
        EnsureKnownKind(rule.KindCode);
        return rule.KindCode == ContractCheckScopeCodes.Kinds.Exclude;
    }

    // Job payload'i dogrudan cagrilsa bile kapsam kural turunun kapali katalogda kalmasini saglar.
    private static void EnsureKnownKind(string kindCode)
    {
        if (!ContractCheckScopeCodes.Kinds.All.Contains(kindCode))
        {
            throw new BusinessException(ContractCheckRunExceptionCodes.InvalidScopeRule);
        }
    }

    // Basit wildcard desenini ordinal ve buyuk/kucuk harf duyarli olarak eslestirir.
    private static bool Matches(string pattern, string value)
    {
        if (string.IsNullOrWhiteSpace(pattern))
        {
            throw new BusinessException(ContractCheckRunExceptionCodes.InvalidScopeRule);
        }

        return FileSystemName.MatchesSimpleExpression(pattern, value, ignoreCase: false);
    }

    // Filtrede kalan operasyonun dokumantasyon hedefleriyle ortak kararli adresini kurar.
    private static string BuildOperationTarget(SpecOperationModel operation)
    {
        return string.Concat(
            operation.Method,
            SpecNormalizationTextConstants.Normalization.SingleSpace,
            operation.Path);
    }

    // Yapisal kapsamdan cikan nesnenin DocsOnly kaydinin geride kalmasini engeller.
    private static bool IsDocumentationIncluded(
        SpecDocumentationModel item,
        IReadOnlySet<string> operationTargets,
        IReadOnlySet<string> schemaTargets)
    {
        return item.TargetKind switch
        {
            SpecNormalizationTextConstants.DocumentationTargets.Operation or
            SpecNormalizationTextConstants.DocumentationTargets.Parameter or
            SpecNormalizationTextConstants.DocumentationTargets.RequestBody or
            SpecNormalizationTextConstants.DocumentationTargets.Response or
            SpecNormalizationTextConstants.DocumentationTargets.Header =>
                operationTargets.Any(target => IsTargetOrChild(item.Target, target)),
            SpecNormalizationTextConstants.DocumentationTargets.Schema or
            SpecNormalizationTextConstants.DocumentationTargets.Property =>
                schemaTargets.Any(target => IsTargetOrChild(item.Target, target)),
            _ => true
        };
    }

    // Dokumantasyon adresinin secili nesnenin kendisi veya ayiracli alt nesnesi oldugunu bildirir.
    private static bool IsTargetOrChild(string candidate, string target)
    {
        return candidate == target || candidate.StartsWith(
            string.Concat(target, SpecNormalizationTextConstants.Normalization.TypeSeparator),
            StringComparison.Ordinal);
    }
}
