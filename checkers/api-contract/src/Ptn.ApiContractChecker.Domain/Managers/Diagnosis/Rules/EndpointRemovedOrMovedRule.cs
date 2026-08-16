using Ptn.ApiContractChecker.Constants.Diagnosis;
using Ptn.ApiContractChecker.Constants.Differences.Lookups;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Diagnosis.Rules;

// islevi: Endpoint kaldirildi veya tasindi hipotezini endpoint-removed bulgusuna baglar.
// sistemdeki gorevi: H-CD-04'u bagimsiz rule sinifi olarak DI kataloguna katar.
public sealed class EndpointRemovedOrMovedRule : FindingDiagnosisRuleBase, ITransientDependency
{
    public override string HypothesisKindCode => HypothesisKindCodes.EndpointRemovedOrMoved;
    protected override IReadOnlyCollection<string> FindingKindCodes { get; } = [DifferenceKindCodes.EndpointRemoved];
}
