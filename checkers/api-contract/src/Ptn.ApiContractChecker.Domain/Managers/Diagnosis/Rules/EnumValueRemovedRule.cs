using Ptn.ApiContractChecker.Constants.Diagnosis;
using Ptn.ApiContractChecker.Constants.Differences.Lookups;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Diagnosis.Rules;

// islevi: Request enum degeri kaldirildi hipotezini ilgili comparison bulgusuna baglar.
// sistemdeki gorevi: H-CD-03'u bagimsiz rule sinifi olarak DI kataloguna katar.
public sealed class EnumValueRemovedRule : FindingDiagnosisRuleBase, ITransientDependency
{
    public override string HypothesisKindCode => HypothesisKindCodes.EnumValueRemoved;
    protected override IReadOnlyCollection<string> FindingKindCodes { get; } =
        [DifferenceKindCodes.RequestParameterEnumValueRemoved];
}
