using Ptn.ApiContractChecker.Constants.Diagnosis;
using Ptn.ApiContractChecker.Constants.Differences.Lookups;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Diagnosis.Rules;

// islevi: Response alani optional veya nullable oldu hipotezini iki comparison bulgusuna baglar.
// sistemdeki gorevi: H-CD-07'yi bagimsiz rule sinifi olarak DI kataloguna katar.
public sealed class PropertyBecameOptionalRule : FindingDiagnosisRuleBase, ITransientDependency
{
    public override string HypothesisKindCode => HypothesisKindCodes.PropertyBecameOptional;
    protected override IReadOnlyCollection<string> FindingKindCodes { get; } =
        [DifferenceKindCodes.ResponsePropertyBecameOptional, DifferenceKindCodes.ResponsePropertyBecameNullable];
}
