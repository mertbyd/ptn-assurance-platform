using Ptn.ApiContractChecker.Constants.Diagnosis;
using Ptn.ApiContractChecker.Constants.Differences.Lookups;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Diagnosis.Rules;

// islevi: Response medya tipi kaldirildi hipotezini comparison bulgusuna baglar.
// sistemdeki gorevi: H-CD-06'yi bagimsiz rule sinifi olarak DI kataloguna katar.
public sealed class MediaTypeRemovedRule : FindingDiagnosisRuleBase, ITransientDependency
{
    public override string HypothesisKindCode => HypothesisKindCodes.MediaTypeRemoved;
    protected override IReadOnlyCollection<string> FindingKindCodes { get; } =
        [DifferenceKindCodes.ResponseMediaTypeRemoved];
}
