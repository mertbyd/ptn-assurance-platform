using Volo.Abp.Authorization.Permissions;
using Ptn.ApiContractChecker.Localization;

namespace Ptn.ApiContractChecker.Permissions;

// islevi: Lookup yetkilerini ABP permission agacina ekler.
// sistemdeki gorevi: Ana provider'in Define metodu bu partial metodu cagirir; yeni modul eklemek mevcut satirlari degistirmeden tek cagri ekler.
public partial class ApiContractCheckerPermissionDefinitionProvider
{
    private void AddLookupsPermissions(PermissionGroupDefinition group)
    {
        var lookups = group.AddPermission(
            ApiContractCheckerPermissions.Lookups.Default,
            L(ApiContractCheckerLocalizationKeys.Permissions.Lookups));

        lookups.AddChild(
            ApiContractCheckerPermissions.Lookups.View,
            L(ApiContractCheckerLocalizationKeys.Permissions.LookupsView));

        lookups.AddChild(
            ApiContractCheckerPermissions.Lookups.Manage,
            L(ApiContractCheckerLocalizationKeys.Permissions.LookupsManage));
    }
}
