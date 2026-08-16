using Ptn.ApiContractChecker.Constants.Diagnosis;
using Ptn.ApiContractChecker.Constants.Differences.Lookups;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Diagnosis.Rules;

// islevi: Yeni zorunlu request alani hipotezini request required bulgularina baglar.
// sistemdeki gorevi: H-CD-02'yi ag cagrisi olmadan finding olgusuyla kesinlestirir.
public sealed class RequiredRequestFieldCreatedRule : FindingDiagnosisRuleBase, ITransientDependency
{
    public override string HypothesisKindCode => HypothesisKindCodes.RequiredRequestFieldCreated;
    protected override IReadOnlyCollection<string> FindingKindCodes { get; } =
        [DifferenceKindCodes.NewRequiredRequestProperty, DifferenceKindCodes.RequestPropertyBecameRequired,
            DifferenceKindCodes.RequestBodyBecameRequired];
}
