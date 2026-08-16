using Ptn.ApiContractChecker.Constants.Diagnosis;
using Ptn.ApiContractChecker.Constants.Differences.Lookups;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Diagnosis.Rules;

// islevi: Basari durum kodu degisikligi hipotezini response-success-status bulgusuna baglar.
// sistemdeki gorevi: H-CD-05'i bagimsiz rule sinifi olarak DI kataloguna katar.
public sealed class SuccessStatusChangedRule : FindingDiagnosisRuleBase, ITransientDependency
{
    public override string HypothesisKindCode => HypothesisKindCodes.SuccessStatusChanged;
    protected override IReadOnlyCollection<string> FindingKindCodes { get; } =
        [DifferenceKindCodes.ResponseSuccessStatusRemoved];
}
