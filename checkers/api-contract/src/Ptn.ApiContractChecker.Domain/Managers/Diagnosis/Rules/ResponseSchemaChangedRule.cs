using Ptn.ApiContractChecker.Constants.Diagnosis;
using Ptn.ApiContractChecker.Constants.Differences.Lookups;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Diagnosis.Rules;

// islevi: Response semasi degisikligi hipotezini schema ve type change bulgularina baglar.
// sistemdeki gorevi: H-CD-01'i bagimsiz ve conventional DI ile genisletilebilir rule olarak saglar.
public sealed class ResponseSchemaChangedRule : FindingDiagnosisRuleBase, ITransientDependency
{
    public override string HypothesisKindCode => HypothesisKindCodes.ResponseSchemaChanged;
    protected override IReadOnlyCollection<string> FindingKindCodes { get; } =
        [DifferenceKindCodes.SchemaRemoved, DifferenceKindCodes.SchemaRenamed, DifferenceKindCodes.RequestPropertyTypeChanged];
}
