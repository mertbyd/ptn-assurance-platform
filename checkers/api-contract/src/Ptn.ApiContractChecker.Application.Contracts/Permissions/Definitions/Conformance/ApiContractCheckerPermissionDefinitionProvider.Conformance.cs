using Ptn.ApiContractChecker.Localization;
using Volo.Abp.Authorization.Permissions;

namespace Ptn.ApiContractChecker.Permissions;

// islevi: Response conformance kosum iznini permission agacina ekler.
// sistemdeki gorevi: Test Module oracle cagrilarini diger contract check yetkilerinden ayirir.
public partial class ApiContractCheckerPermissionDefinitionProvider
{
    private void AddConformancePermissions(PermissionGroupDefinition group)
    {
        var conformance = group.AddPermission(
            ApiContractCheckerPermissions.Conformance.Default,
            L(ApiContractCheckerLocalizationKeys.Permissions.Conformance));
        conformance.AddChild(
            ApiContractCheckerPermissions.Conformance.Execute,
            L(ApiContractCheckerLocalizationKeys.Permissions.ConformanceExecute));
        conformance.AddChild(
            ApiContractCheckerPermissions.Conformance.GenerateSamples,
            L(ApiContractCheckerLocalizationKeys.Permissions.ConformanceGenerateSamples));
        conformance.AddChild(
            ApiContractCheckerPermissions.Conformance.SuggestLinks,
            L(ApiContractCheckerLocalizationKeys.Permissions.ConformanceSuggestLinks));
    }
}
