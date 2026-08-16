using Ptn.ApiContractChecker.Localization;
using Volo.Abp.Authorization.Permissions;

namespace Ptn.ApiContractChecker.Permissions;

// islevi: Diagnosis execute iznini API Contract Checker permission agacina ekler.
// sistemdeki gorevi: Test Module diagnosis cagrilarini ayri policy ile yetkilendirir.
public partial class ApiContractCheckerPermissionDefinitionProvider
{
    private void AddDiagnosisPermissions(PermissionGroupDefinition group)
    {
        var diagnosis = group.AddPermission(
            ApiContractCheckerPermissions.Diagnosis.Default,
            L(ApiContractCheckerLocalizationKeys.Permissions.Diagnosis));
        diagnosis.AddChild(
            ApiContractCheckerPermissions.Diagnosis.Execute,
            L(ApiContractCheckerLocalizationKeys.Permissions.DiagnosisExecute));
    }
}
