using Volo.Abp.Authorization.Permissions;
using Ptn.ApiContractChecker.Localization;

namespace Ptn.ApiContractChecker.Permissions;

// islevi: SpecSource ve aggregate dokuman yonetimi yetkilerini ABP permission agacina ekler.
// sistemdeki gorevi: KBP-607 HTTP yuzeyinin kullanacagi View ve Manage sabitlerini calisma zamaninda tanimlar.
public partial class ApiContractCheckerPermissionDefinitionProvider
{
    // Kaynak okuma ve yonetim izinlerini ana permission grubuna ekler.
    private void AddSourcesPermissions(PermissionGroupDefinition group)
    {
        var sources = group.AddPermission(ApiContractCheckerPermissions.Sources.Default, L(ApiContractCheckerLocalizationKeys.Permissions.Sources));
        sources.AddChild(ApiContractCheckerPermissions.Sources.View, L(ApiContractCheckerLocalizationKeys.Permissions.SourcesView));
        sources.AddChild(ApiContractCheckerPermissions.Sources.Manage, L(ApiContractCheckerLocalizationKeys.Permissions.SourcesManage));
    }
}
