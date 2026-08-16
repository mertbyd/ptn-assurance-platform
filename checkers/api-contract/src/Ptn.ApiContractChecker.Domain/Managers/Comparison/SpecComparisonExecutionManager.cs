using Ptn.ApiContractChecker.Managers.Snapshots;
using Ptn.ApiContractChecker.Models.Comparison;
using Ptn.ApiContractChecker.Models.Runs;
using Ptn.ApiContractChecker.Models.Snapshots;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace Ptn.ApiContractChecker.Managers.Comparison;

// islevi: Iki spec snapshot'ini normalize edip operasyon ve sema farklarini siddetlendirilmis bulgulara cevirir.
// sistemdeki gorevi: Saf comparison pipeline'inin ContractCheckRun.Findings'e dogrudan verilebilen tek giris noktasidir.
public class SpecComparisonExecutionManager : DomainService
{
    private readonly SpecSnapshotNormalizer _snapshotNormalizer;
    private readonly SpecOperationComparisonManager _operationComparisonManager;
    private readonly SpecSchemaComparisonManager _schemaComparisonManager;
    private readonly SpecDifferenceSeverityClassifier _severityClassifier;
    private readonly SpecCollectionComparer _collectionComparer;
    private readonly SpecComparisonScopeManager _scopeManager;

    // Comparison pipeline'inin mevcut saf adimlarini acik bagimliliklarla birlestirir.
    public SpecComparisonExecutionManager(
        SpecSnapshotNormalizer snapshotNormalizer,
        SpecOperationComparisonManager operationComparisonManager,
        SpecSchemaComparisonManager schemaComparisonManager,
        SpecDifferenceSeverityClassifier severityClassifier,
        SpecCollectionComparer collectionComparer,
        SpecComparisonScopeManager scopeManager)
    {
        _snapshotNormalizer = snapshotNormalizer;
        _operationComparisonManager = operationComparisonManager;
        _schemaComparisonManager = schemaComparisonManager;
        _severityClassifier = severityClassifier;
        _collectionComparer = collectionComparer;
        _scopeManager = scopeManager;
    }

    // Base ve target snapshot'larini tek kez normalize edip tum farklari kararli bulgu govdesine indirger.
    public ContractCheckFindings Compare(
        SpecSnapshotModel baseSnapshot,
        SpecSnapshotModel targetSnapshot)
    {
        return Compare(baseSnapshot, targetSnapshot, [], ignoreInternal: false);
    }

    // Snapshot'lari tek kez normalize eder, gecici kapsami uygular ve tum farklari kararli bulgu govdesine indirger.
    public ContractCheckFindings Compare(
        SpecSnapshotModel baseSnapshot,
        SpecSnapshotModel targetSnapshot,
        IReadOnlyCollection<ContractCheckScopeRuleModel> scopeRules,
        bool ignoreInternal)
    {
        var normalizedBase = _snapshotNormalizer.Normalize(Check.NotNull(baseSnapshot, nameof(baseSnapshot)));
        var normalizedTarget = _snapshotNormalizer.Normalize(Check.NotNull(targetSnapshot, nameof(targetSnapshot)));
        var scopedBase = _scopeManager.Apply(normalizedBase, scopeRules, ignoreInternal);
        var scopedTarget = _scopeManager.Apply(normalizedTarget, scopeRules, ignoreInternal);
        var differences = _operationComparisonManager.Compare(scopedBase, scopedTarget);
        differences.AddRange(_schemaComparisonManager.Compare(scopedBase, scopedTarget));

        return new ContractCheckFindings(
            _collectionComparer.SortDifferences(differences).Select(ConvertToFinding));
    }

    // Siddet kararini ekleyip ara modelin tum alanlarini owned finding sozlesmesine kayipsiz tasir.
    private Finding ConvertToFinding(SpecDifferenceModel difference)
    {
        return new Finding(
            difference.KindCode,
            _severityClassifier.Classify(difference),
            difference.DirectionCode,
            difference.Address,
            difference.OldValue,
            difference.NewValue);
    }
}
